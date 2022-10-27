using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using Newtonsoft.Json;
using RandomizerMod.Menu;
using RandoSettingsManager.Model;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Tar;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RandoSettingsManager.Menu
{
    internal class MenuManager
    {
        private static SmallButton? quickShareCreate;
        private static SmallButton? quickShareLoad;
        private static Messager? messager;
        private const string quickShareServiceUrl = "https://wakqqsjpt464rapvz5rz4po3pm0ucgty.lambda-url.us-west-2.on.aws/";
        private static readonly HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static void HookMenu()
        {
            RandomizerMenuAPI.AddMenuPage(Construct, NoOpConstructButton!);
        }

        private static void Construct(MenuPage connectionPage)
        {
            RandomizerMenu rm = RandomizerMenuAPI.Menu;
            SmallButton manageBtn = ReflectionHelper.GetField<RandomizerMenu, SmallButton>(
                rm, "ToManageSettingsPageButton");

            MenuPage classic = ReflectionHelper.GetField<RandomizerMenu, MenuPage>(
                rm, "ManageSettingsPage");

            MenuPage modern = new("RandoSettingsManager Manage Settings", manageBtn.Parent);

            PatchRandoMenuPages(manageBtn, classic, modern);
            BuildManagePage(classic, modern);
        }

        private static void BuildManagePage(MenuPage classic, MenuPage modern)
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
            quickShareCreate = new(modern, "Create Key");
            quickShareCreate.OnClick += CreateKeyClick;
            quickShareLoad = new(modern, "Paste Key");
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

            messager = new(modern);
            messager.MoveTo(new Vector2(0, 125));

            modern.AfterHide += () =>
            {
                messager.Clear();
            };

            new GridItemPanel(modern, SpaceParameters.TOP_CENTER_UNDER_TITLE + new Vector2(0, -SpaceParameters.VSPACE_MEDIUM),
                2, 0, SpaceParameters.HSPACE_LARGE, true,
                quickShareVip, profileVip);
        }

        private static void PatchRandoMenuPages(SmallButton manageButton,
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

        private static void CreateKeyClick()
        {
            quickShareCreate!.Lock();
            quickShareLoad!.Lock();
            messager!.Clear();
            messager.Write("Creating settings key...");

            new Thread(DoCreateSettings).Start();
        }

        private static void DoCreateSettings()
        {
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
                messager!.Clear();
                messager.Write("An unexpected error occurred while creating settings key.");
                quickShareCreate!.Unlock();
                quickShareLoad!.Unlock();
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
                        messager!.Clear();
                        messager.WriteLine("Created settings code and copied to clipboard!");
                        messager.Write(resp.SettingsKey);
                    });
                }
                else
                {
                    RandoSettingsManagerMod.Instance.LogError($"An unexpected response was received while " +
                        $"creating settings key: {respContent.Result}");
                    ThreadSupport.BeginInvoke(() =>
                    {
                        messager!.Clear();
                        messager.Write($"An unexpected response was received while creating settings key.");
                    });
                }
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                ThreadSupport.BeginInvoke(() =>
                {
                    messager!.Clear();
                    messager.Write("Failed to create settings key.");
                });
            }
            finally
            {
                ThreadSupport.BeginInvoke(() =>
                {
                    quickShareCreate!.Unlock();
                    quickShareLoad!.Unlock();
                });
            }
        }

        private static void LoadKeyClick()
        {
            string key = GUIUtility.systemCopyBuffer.Trim();

            quickShareCreate!.Lock();
            quickShareLoad!.Lock();
            messager!.Clear();
            messager.Write($"Looking up settings from key {key}...");

            new Thread(() => DoLoadSettings(key)).Start();
        }

        private static void DoLoadSettings(string key)
        {
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
                        messager!.Clear();
                        messager.Write($"An unexpected response was received while reading settings from key {key}: {t.Result}");
                        quickShareCreate?.Unlock();
                        quickShareLoad?.Unlock();
                    });
                    return;
                }
                else if (!resp.Found || resp.Settings == null)
                {
                    ThreadSupport.BeginInvoke(() =>
                    {
                        messager!.Clear();
                        messager.Write($"Couldn't find settings with key {key}");
                        quickShareCreate?.Unlock();
                        quickShareLoad?.Unlock();
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
                    messager!.Clear();
                    messager.Write($"An unexpected error occurred while reading settings from key {key}");
                    quickShareCreate?.Unlock();
                    quickShareLoad?.Unlock();
                });
                return;
            }

            try
            {
                using MemoryStream ms = new(settings);
                TgzFiler filer = TgzFiler.LoadFromStream(ms);

                ThreadSupport.BlockUntilInvoked(() =>
                {
                    RandoSettingsManagerMod.Instance.settingsManager?.LoadSettings(filer.RootDirectory, true);
                });

                ThreadSupport.BeginInvoke(() =>
                {
                    messager!.Clear();
                    messager.Write($"Successfully loaded settings from key {key}");
                });
                
            }
            catch (ValidationException ve)
            {
                ThreadSupport.BeginInvoke(() =>
                {
                    messager!.Clear();
                    messager.WriteLine($"The settings provided by key {key} failed validation!");
                    messager.Write(ve.Message);
                });
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError(ex);
                ThreadSupport.BeginInvoke(() =>
                {
                    messager!.Clear();
                    messager.Write($"An unexpected error occurred loading settings from key {key}");
                });
            }
            finally
            {
                quickShareCreate?.Unlock();
                quickShareLoad?.Unlock();
            }
        }
    }
}
