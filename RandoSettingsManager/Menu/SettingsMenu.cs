﻿using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using Newtonsoft.Json;
using RandomizerCore.Extensions;
using RandomizerMod.Menu;
using RandoSettingsManager.Model;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Tar;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RandoSettingsManager.Menu
{
    internal class SettingsMenu
    {
        private const string quickShareServiceUrl = "https://wakqqsjpt464rapvz5rz4po3pm0ucgty.lambda-url.us-west-2.on.aws/";
        private const string tempProfileName = "rando-temp-profile.tar.gz";
        private static readonly HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        public static readonly string ProfilesDir = Path.Combine(Application.persistentDataPath, "Randomizer 4", "Profiles");
        private static readonly string TempProfilePath = Path.Combine(ProfilesDir, tempProfileName);

        private readonly FileSystemWatcher tempWatcher;

        private SmallButton navToClassic;
        private SmallButton quickShareCreate;
        private SmallButton quickShareLoad;
        private SmallButton quickShareCancel;
        private SmallButton manageProfiles;
        private SmallButton createTempProfile;
        private SmallButton disableConnections;
        private Messager messager;

        private readonly SmallButton backButton;

        private CancellationTokenSource? cancellationTokenSource;

        private SettingsMenu()
        {
            RandomizerMenu rm = RandomizerMenuAPI.Menu;
            SmallButton manageBtn = ReflectionHelper.GetField<RandomizerMenu, SmallButton>(
                rm, "ToManageSettingsPageButton");

            MenuPage classic = ReflectionHelper.GetField<RandomizerMenu, MenuPage>(
                rm, "ManageSettingsPage");

            MenuPage modern = new("RandoSettingsManager Manage Settings", manageBtn.Parent);

            backButton = modern.backButton;

            BuildManagePage(classic, modern); 
            PatchRandoMenuPages(manageBtn, classic, modern);
            
            Directory.CreateDirectory(ProfilesDir);
            tempWatcher = new FileSystemWatcher(ProfilesDir, "*.tar.gz")
            {
                EnableRaisingEvents = false,
                NotifyFilter = NotifyFilters.CreationTime
            };
            tempWatcher.Created += OnFileCreated;
        }

        public static void HookMenu()
        {
            RandomizerMenuAPI.AddMenuPage(page => new SettingsMenu(), NoOpConstructButton!);
        }

        [MemberNotNull(
            nameof(navToClassic), 
            nameof(quickShareCreate), nameof(quickShareLoad), nameof(quickShareCancel),
            nameof(manageProfiles), nameof(createTempProfile), nameof(disableConnections), 
            nameof(messager)
        )]
        private void BuildManagePage(MenuPage classic, MenuPage modern)
        {
            ProfilesPage profiles = new(modern);

            navToClassic = new(modern, "Classic Settings Management");
            navToClassic.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            modern.ReplaceNavigation(new HorizontalNavWithItemAboveBackButton(modern, navToClassic));

            navToClassic.OnClick += () =>
            {
                RandoSettingsManagerMod.Instance.GS.Mode = SettingsManagementMode.Classic;
                VisitSettingsPageForCurrentMode(navToClassic, classic, modern);
            };

            ColumnHeader quickShareHeader = new(modern, "Quick Share");
            quickShareCreate = new(modern, "Create Key");
            quickShareCreate.OnClick += CreateKeyClick;
            quickShareLoad = new(modern, "Paste Key");
            quickShareLoad.OnClick += LoadKeyClick;
            quickShareCancel = new(modern, "Cancel");
            quickShareCancel.OnClick += CancelClick;
            quickShareCancel.Hide();

            VerticalItemPanel quickShareVip = new(modern, Vector2.zero, SpaceParameters.VSPACE_SMALL, false,
                quickShareCreate,
                quickShareLoad,
                quickShareCancel);

            ColumnHeader profileHeader = new(modern, "Profiles");
            manageProfiles = new(modern, "Manage Profiles");
            createTempProfile = new(modern, "Create Temporary Profile");
            disableConnections = new(modern, "Disable Connections");

            manageProfiles.AddHideAndShowEvent(profiles.RootPage);
            createTempProfile.OnClick += CreateTempProfileClick;
            disableConnections.OnClick += DisableConnectionsClick;

            VerticalItemPanel profileVip = new(modern, Vector2.zero, SpaceParameters.VSPACE_SMALL, false,
                manageProfiles,
                createTempProfile,
                disableConnections);

            quickShareHeader.MoveTo(new Vector2(-SpaceParameters.HSPACE_LARGE / 2, 300));
            profileHeader.MoveTo(new Vector2(SpaceParameters.HSPACE_LARGE / 2, 300));

            messager = new(modern);
            messager.MoveTo(new Vector2(0, 95));

            modern.AfterShow += async () =>
            {
                tempWatcher.EnableRaisingEvents = true;
                if (File.Exists(TempProfilePath))
                {
                    LockMenu();
                    messager.Clear();
                    messager.Write($"Attempting to load temporary profile from {TempProfilePath}");
                    await LoadTempProfileAsync();
                }
            };

            modern.AfterHide += () =>
            {
                messager.Clear();
                tempWatcher.EnableRaisingEvents = false;
                try
                {
                    if (File.Exists(TempProfilePath))
                    {
                        File.Delete(TempProfilePath);
                    }
                }
                catch (Exception ex)
                {
                    RandoSettingsManagerMod.Instance.LogWarn($"Failed to delete temp profile: {ex}");
                }
            };

            new GridItemPanel(modern, SpaceParameters.TOP_CENTER_UNDER_TITLE + new Vector2(0, -SpaceParameters.VSPACE_MEDIUM),
                2, 0, SpaceParameters.HSPACE_LARGE, true,
                quickShareVip, profileVip);
        }

        private void PatchRandoMenuPages(SmallButton manageButton,
            MenuPage classic, MenuPage modern)
        {
            ReflectionHelper.SetField<BaseButton, Action?>(manageButton, nameof(SmallButton.OnClick), null);

            SmallButton navToModern = new(classic, "Modern Settings Management");
            navToModern.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            classic.ReplaceNavigation(new HorizontalNavWithItemAboveBackButton(classic, navToModern));

            manageButton.OnClick += () => VisitSettingsPageForCurrentMode(manageButton, classic, modern);
            navToModern.OnClick += () =>
            {
                RandoSettingsManagerMod.Instance.GS.Mode = SettingsManagementMode.Modern;
                VisitSettingsPageForCurrentMode(navToModern, classic, modern);
            };
        }

        private async void CreateKeyClick()
        {
            LockMenu();
            quickShareCancel.Show();
            messager.Clear();
            messager.Write("Creating settings key...");

            await CreateSettingsAsync();
        }

        private async Task CreateSettingsAsync()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            byte[] settings;
            try
            {
                TgzFiler filer = TgzFiler.CreateForWrite();

                settings = await Task.Run(() =>
                {
                    manager.SaveSettings(filer.RootDirectory, true, true);

                    using MemoryStream ms = new();
                    filer.WriteAll(ms);

                    return ms.ToArray();
                });
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write("An unexpected error occurred while creating settings key.");
                quickShareCancel.Hide();
                UnlockMenu();
                return;
            }

            string encodedSettings = Convert.ToBase64String(settings);

            try
            {
                string reqContentS = JsonConvert.SerializeObject(new CreateSettingsInput() { Settings = encodedSettings });
                cancellationTokenSource = new CancellationTokenSource();
                StringContent reqContent = new(reqContentS, Encoding.UTF8, "application/json");
                HttpResponseMessage postResp = await httpClient.PostAsync(quickShareServiceUrl, reqContent, cancellationTokenSource.Token);
                string respContent = await postResp.Content.ReadAsStringAsync();

                CreateSettingsOutput? resp = JsonConvert.DeserializeObject<CreateSettingsOutput>(respContent);
                if (resp != null)
                {
                    GUIUtility.systemCopyBuffer = resp.SettingsKey;
                    messager.Clear();
                    messager.WriteLine("Created settings code and copied to clipboard!");
                    messager.WriteLine(resp.SettingsKey);
                    messager.Write($"Settings were shared for {ListJoin(manager.LastSentMods)}. Settings for other " +
                        $"connections must be shared manually if they're enabled.");
                }
                else
                {
                    RandoSettingsManagerMod.Instance.LogError($"An unexpected response was received while " +
                        $"creating settings key: {respContent}");
                    messager.Clear();
                    messager.Write($"An unexpected response was received while creating settings key.");
                }
            }
            catch (OperationCanceledException ex)
            {
                messager.Clear();
                messager.WriteLine("Creating settings key cancelled or timed out.");
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write("Failed to create settings key over the network.");
            }
            finally
            {
                quickShareCancel.Hide();
                UnlockMenu();
            }
        }

        private async void LoadKeyClick()
        {
            string key = GUIUtility.systemCopyBuffer.Trim();

            LockMenu();
            quickShareCancel.Show();
            messager.Clear();
            messager.Write($"Looking up settings from key {key}...");

            await LoadSettingsAsync(key);
        }

        private async Task LoadSettingsAsync(string key)
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            byte[] settings;
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                HttpResponseMessage getSettingsResponse = await httpClient.GetAsync(quickShareServiceUrl 
                    + new RetrieveSettingsInput() { SettingsKey = key }.ToQueryString(), cancellationTokenSource.Token);
                string serializedSettings = await getSettingsResponse.Content.ReadAsStringAsync();

                RetrieveSettingsOutput? resp = JsonConvert.DeserializeObject<RetrieveSettingsOutput>(serializedSettings);
                if (resp == null)
                {
                    messager.Clear();
                    messager.Write($"An unexpected response was received while reading settings from key {key}: {serializedSettings}");
                    quickShareCancel.Hide();
                    UnlockMenu();
                    return;
                }
                else if (!resp.Found || resp.Settings == null)
                {
                    messager.Clear();
                    messager.Write($"Couldn't find settings with key {key}");
                    quickShareCancel.Hide();
                    UnlockMenu();
                    return;
                }
                else
                {
                    settings = Convert.FromBase64String(resp.Settings);
                }
            }
            catch (OperationCanceledException)
            {
                messager.Clear();
                messager.WriteLine("Loading settings key cancelled or timed out.");
                quickShareCancel.Hide();
                UnlockMenu();
                return;
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write($"An unexpected error occurred while reading settings from key {key}");
                quickShareCancel.Hide();
                UnlockMenu();
                return;
            }

            try
            {
                using MemoryStream ms = new(settings);
                TgzFiler filer = TgzFiler.LoadFromStream(ms);

                manager.LoadSettings(filer.RootDirectory, true);

                WriteReceivedSettingsToMessager(manager, $"Successfully loaded settings from key {key}");
            }
            catch (ValidationException ve)
            {
                messager.Clear();
                messager.WriteLine($"The settings provided by key {key} failed validation!");
                messager.Write(ve.Message);
            }
            catch (LateValidationException ve)
            {
                WriteReceivedSettingsToMessager(manager, ve.Message);
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write($"An unexpected error occurred loading settings from key {key}");
            }
            finally
            {
                quickShareCancel.Hide();
                UnlockMenu();
            }
        }

        private void CancelClick()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private async void CreateTempProfileClick()
        {
            LockMenu();
            messager.Clear();
            messager.Write("Creating temporary profile...");

            await CreateTempProfileAsync();
        }

        private async Task CreateTempProfileAsync()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            try
            {
                TgzFiler filer = TgzFiler.CreateForWrite();

                await Task.Run(() =>
                {
                    manager.SaveSettings(filer.RootDirectory, true, true);
                    using (FileStream fs = File.Create(TempProfilePath))
                    {
                        filer.WriteAll(fs);
                    }
                });

                System.Diagnostics.Process.Start(ProfilesDir);
                messager.Clear();
                messager.Write($"Successfully wrote temp profile to {TempProfilePath}");
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write("An unexpected error occurred while creating a temporary profile.");
            }
            finally
            {
                UnlockMenu();
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            RandoSettingsManagerMod.Instance.LogDebug($"Saw {e.Name} created");
            // if the file matches, lock up and queue it for extraction
            if (Path.GetFileName(e.Name) == tempProfileName)
            {
                // profile load needs to start on the main thread
                ThreadSupport.BlockUntilInvoked(async () =>
                {
                    LockMenu();
                    messager.Clear();
                    messager.Write($"Attempting to load temporary profile from {TempProfilePath}");
                    await LoadTempProfileAsync();
                });
            }
        }

        private async Task LoadTempProfileAsync()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;

            try
            {
                TgzFiler filer = await Task.Run(() =>
                {
                    using (FileStream fs = File.OpenRead(TempProfilePath))
                    {
                        return TgzFiler.LoadFromStream(fs);
                    }
                });

                manager.LoadSettings(filer.RootDirectory, true);

                File.Delete(TempProfilePath);

                WriteReceivedSettingsToMessager(manager, "Successfully loaded settings from the temporary profile!");
            }
            catch (ValidationException ve)
            {
                messager.Clear();
                messager.WriteLine($"The settings loaded from the temporary profile failed validation!");
                messager.Write(ve.Message);
            }
            catch (LateValidationException ve)
            {
                WriteReceivedSettingsToMessager(manager, ve.Message);
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write("An unexpected error occurred while loading a temporary profile.");
            }
            finally
            {
                UnlockMenu();
            }
        }

        private void DisableConnectionsClick()
        {
            SettingsManager manager = RandoSettingsManagerMod.Instance.settingsManager;
            try
            {
                manager.DisableAllConnections();
                WriteReceivedSettingsToMessager(manager, "Successfully disabled connections!");
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                messager.Clear();
                messager.Write("An unexpected error occurred while disabling connections.");
            }
        }

        private void LockMenu()
        {
            tempWatcher.EnableRaisingEvents = false;
            quickShareCreate.Lock();
            quickShareLoad.Lock();
            manageProfiles.Lock();
            createTempProfile.Lock();
            disableConnections.Lock();
            navToClassic.Lock();
            backButton.Lock();
        }

        private void UnlockMenu()
        {
            quickShareCreate.Unlock();
            quickShareLoad.Unlock();
            manageProfiles.Unlock();
            createTempProfile.Unlock();
            navToClassic.Unlock();
            disableConnections.Unlock();
            backButton.Unlock();
            tempWatcher.EnableRaisingEvents = true;
        }

        private void WriteReceivedSettingsToMessager(SettingsManager manager, string statusMessage)
        {
            messager.Clear();
            messager.WriteLine(statusMessage);
            if (manager.LastReceivedMods.Count > 0)
            {
                messager.WriteLine($"Settings were received for the following mods: {ListJoin(manager.LastReceivedMods)}.");
            }
            if (manager.LastModsReceivedWithoutSettings.Count > 0)
            {
                messager.WriteLine($"The following mods were disabled: {ListJoin(manager.LastModsReceivedWithoutSettings)}.");
            }
            messager.Write($"Other connections must be configured manually.");
        }

        private static void VisitSettingsPageForCurrentMode(SmallButton sender, MenuPage classic, MenuPage modern)
        {
            sender.Parent.Hide();
            switch (RandoSettingsManagerMod.Instance.GS.Mode)
            {
                case SettingsManagementMode.Modern:
                    modern.Show();
                    break;
                case SettingsManagementMode.Classic:
                    classic.Show();
                    break;
                default:
                    throw new NotImplementedException($"Missing mode handler for {RandoSettingsManagerMod.Instance.GS.Mode}");
            }
        }

        private static bool NoOpConstructButton(MenuPage connectionPage, out SmallButton? b)
        {
            b = null;
            return false;
        }

        private static string ListJoin(List<string> strings)
        {
            if (strings.Count == 0)
            {
                return "";
            }
            else if (strings.Count == 1)
            {
                return strings[0];
            }
            else if (strings.Count == 2)
            {
                return string.Join(" and ", strings);
            }
            else
            {
                return string.Join(", ", strings.Slice(0, strings.Count - 1)) + ", and " + strings[strings.Count - 1];
            }
        }
    }
}
