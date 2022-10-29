using MenuChanger;
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
        private static readonly HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        private readonly SmallButton navToClassic;
        private readonly SmallButton quickShareCreate;
        private readonly SmallButton quickShareLoad;
        private readonly SmallButton manageProfiles;
        private readonly SmallButton createTempProfile;
        private readonly Messager messager;

        private SmallButton backButton;

        private SettingsMenu()
        {
            RandomizerMenu rm = RandomizerMenuAPI.Menu;
            SmallButton manageBtn = ReflectionHelper.GetField<RandomizerMenu, SmallButton>(
                rm, "ToManageSettingsPageButton");

            MenuPage classic = ReflectionHelper.GetField<RandomizerMenu, MenuPage>(
                rm, "ManageSettingsPage");

            MenuPage modern = new("RandoSettingsManager Manage Settings", manageBtn.Parent);

            backButton = modern.backButton;

            (navToClassic, quickShareCreate, quickShareLoad, manageProfiles, createTempProfile, messager)
                = BuildManagePage(classic, modern); 
            PatchRandoMenuPages(manageBtn, classic, modern);
            
        }

        public static void HookMenu()
        {
            RandomizerMenuAPI.AddMenuPage(page => new SettingsMenu(), NoOpConstructButton!);
        }

        private (SmallButton, SmallButton, SmallButton, SmallButton, SmallButton, Messager) 
            BuildManagePage(MenuPage classic, MenuPage modern)
        {
            SmallButton navToClassic = new(modern, "Classic Settings Management");
            navToClassic.MoveTo(new(0, -450 + SpaceParameters.VSPACE_SMALL));
            modern.ReplaceNavigation(new HorizontalNavWithItemAboveBackButton(modern, navToClassic));

            navToClassic.OnClick += () =>
            {
                RandoSettingsManagerMod.Instance.GS.Mode = SettingsManagementMode.Classic;
                VisitSettingsPageForCurrentMode(navToClassic, classic, modern);
            };

            ColumnHeader quickShareHeader = new(modern, "Quick Share");
            SmallButton quickShareCreate = new(modern, "Create Key");
            quickShareCreate.OnClick += CreateKeyClick;
            SmallButton quickShareLoad = new(modern, "Paste Key");
            quickShareLoad.OnClick += LoadKeyClick;

            VerticalItemPanel quickShareVip = new(modern, Vector2.zero, SpaceParameters.VSPACE_SMALL, false,
                quickShareCreate,
                quickShareLoad);

            ColumnHeader profileHeader = new(modern, "Profiles");
            SmallButton manageProfiles = new(modern, "Manage Profiles");
            SmallButton createTempProfile = new(modern, "Create Temporary Profile");

            VerticalItemPanel profileVip = new(modern, Vector2.zero, SpaceParameters.VSPACE_SMALL, false,
                manageProfiles,
                createTempProfile);

            quickShareHeader.MoveTo(new Vector2(-SpaceParameters.HSPACE_LARGE / 2, 300));
            profileHeader.MoveTo(new Vector2(SpaceParameters.HSPACE_LARGE / 2, 300));

            Messager messager = new(modern);
            messager.MoveTo(new Vector2(0, 125));

            modern.AfterHide += () =>
            {
                messager.Clear();
            };

            new GridItemPanel(modern, SpaceParameters.TOP_CENTER_UNDER_TITLE + new Vector2(0, -SpaceParameters.VSPACE_MEDIUM),
                2, 0, SpaceParameters.HSPACE_LARGE, true,
                quickShareVip, profileVip);

            return (navToClassic, quickShareCreate, quickShareLoad, manageProfiles, createTempProfile, messager);
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

        private void CreateKeyClick()
        {
            LockMenu();
            messager.Clear();
            messager.Write("Creating settings key...");

            new Thread(DoCreateSettings).Start();
        }

        private void DoCreateSettings()
        {
            SettingsManager? manager = RandoSettingsManagerMod.Instance.settingsManager;
            if (manager == null)
            {
                RandoSettingsManagerMod.Instance.LogError("SettingsManager was null when loading settings");
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.Write($"An unexpected error occurred while creating settings key.");
                    UnlockMenu();
                });
                return;
            }

            byte[] settings;
            try
            {
                TgzFiler filer = TgzFiler.CreateForWrite();
                RandoSettingsManagerMod.Instance.settingsManager?.SaveSettings(filer.RootDirectory, true, true);

                using MemoryStream ms = new();
                filer.WriteAll(ms);

                settings = ms.ToArray();
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.Write("An unexpected error occurred while creating settings key.");
                    UnlockMenu();
                });
                return;
            }

            string encodedSettings = Convert.ToBase64String(settings);

            try
            {
                string reqContentS = JsonConvert.SerializeObject(new CreateSettingsInput() { Settings = encodedSettings });
                StringContent reqContent = new(reqContentS, Encoding.UTF8, "application/json");
                Task<HttpResponseMessage> t = httpClient.PostAsync(quickShareServiceUrl, reqContent);
                t.Wait();

                Task<string> respContent = t.Result.Content.ReadAsStringAsync();
                respContent.Wait();

                CreateSettingsOutput? resp = JsonConvert.DeserializeObject<CreateSettingsOutput>(respContent.Result);
                if (resp != null)
                {
                    ThreadSupport.BeginInvoke(() =>
                    {
                        GUIUtility.systemCopyBuffer = resp.SettingsKey;
                        messager.Clear();
                        messager.WriteLine("Created settings code and copied to clipboard!");
                        messager.WriteLine(resp.SettingsKey);
                        messager.Write($"Settings were shared for {ListJoin(manager.LastSentMods)}. Settings for other " +
                            $"connections must be shared manually if they're enabled.");
                    });
                }
                else
                {
                    RandoSettingsManagerMod.Instance.LogError($"An unexpected response was received while " +
                        $"creating settings key: {respContent.Result}");
                    ThreadSupport.BeginInvoke(() =>
                    {
                        messager.Clear();
                        messager.Write($"An unexpected response was received while creating settings key.");
                    });
                }
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.Write("Failed to create settings key.");
                });
            }
            finally
            {
                ThreadSupport.BeginInvoke(() => UnlockMenu());
            }
        }

        private void LoadKeyClick()
        {
            string key = GUIUtility.systemCopyBuffer.Trim();

            LockMenu();
            messager.Clear();
            messager.Write($"Looking up settings from key {key}...");

            new Thread(() => DoLoadSettings(key)).Start();
        }

        private void DoLoadSettings(string key)
        {
            SettingsManager? manager = RandoSettingsManagerMod.Instance.settingsManager;
            if (manager == null)
            {
                RandoSettingsManagerMod.Instance.LogError("SettingsManager was null when loading settings");
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.Write($"An unexpected error occurred loading settings from key {key}");
                    UnlockMenu();
                });
                return;
            }

            byte[] settings;
            try
            {
                Task<string> t = httpClient.GetStringAsync(quickShareServiceUrl 
                    + new RetrieveSettingsInput() { SettingsKey = key }.ToQueryString());
                t.Wait();

                RetrieveSettingsOutput? resp = JsonConvert.DeserializeObject<RetrieveSettingsOutput>(t.Result);
                if (resp == null)
                {
                    ThreadSupport.BeginInvoke(() =>
                    {
                        messager.Clear();
                        messager.Write($"An unexpected response was received while reading settings from key {key}: {t.Result}");
                        UnlockMenu();
                    });
                    return;
                }
                else if (!resp.Found || resp.Settings == null)
                {
                    ThreadSupport.BeginInvoke(() =>
                    {
                        messager.Clear();
                        messager.Write($"Couldn't find settings with key {key}");
                        UnlockMenu();
                    });
                    return;
                }
                else
                {
                    settings = Convert.FromBase64String(resp.Settings);
                }
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.Write($"An unexpected error occurred while reading settings from key {key}");
                    UnlockMenu();
                });
                return;
            }

            try
            {
                using MemoryStream ms = new(settings);
                TgzFiler filer = TgzFiler.LoadFromStream(ms);

                ThreadSupport.BlockUntilInvoked(() =>
                {
                    manager.LoadSettings(filer.RootDirectory, true);

                    messager.Clear();
                    messager.WriteLine($"Successfully loaded settings from key {key}");
                    messager.Write($"Settings were received for {ListJoin(manager.LastReceivedMods)}. ");
                    if (manager.LastModsReceivedWithoutSettings.Count > 0)
                    {
                        messager.Write($"{ListJoin(manager.LastModsReceivedWithoutSettings)} received no settings and were disabled. ");
                    }
                    messager.Write($"Other connections must be configured manually.");
                });
            }
            catch (ValidationException ve)
            {
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.WriteLine($"The settings provided by key {key} failed validation!");
                    messager.Write(ve.Message);
                });
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                ThreadSupport.BeginInvoke(() =>
                {
                    messager.Clear();
                    messager.Write($"An unexpected error occurred loading settings from key {key}");
                });
            }
            finally
            {
                ThreadSupport.BeginInvoke(() => UnlockMenu());
            }
        }

        private void LockMenu()
        {
            quickShareCreate.Lock();
            quickShareLoad.Lock();
            manageProfiles.Lock();
            createTempProfile.Lock();
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
            backButton.Unlock();
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
