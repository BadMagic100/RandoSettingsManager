using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerMod.Menu;
using RandoSettingsManager.Menu;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Disk;
using RandoSettingsManager.SettingsManagement.Filer.Tar;
using RandoSettingsManager.Testing;
using System;
using System.IO;
using UnityEngine;

namespace RandoSettingsManager
{
    public class RandoSettingsManagerMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal SettingsManager? settingsManager;
        private static RandoSettingsManagerMod? _instance;

        private static readonly string presetPath = Path.Combine(Application.persistentDataPath, "Randomizer 4", "Presets");
        private static readonly DiskFiler dFiler = new(presetPath);

        public GlobalSettings GS { get; private set; } = new();

        public static RandoSettingsManagerMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(RandoSettingsManagerMod)} was never constructed");
                }
                return _instance;
            }
        }

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public RandoSettingsManagerMod() : base("RandoSettingsManager")
        {
            _instance = this;
        }

        public override void Initialize()
        {
            Log("Initializing");

            // create menus and such
            MenuManager.HookMenu();
            RegisterConnection(new TestSettingsProxy());

            RandomizerMenuAPI.AddMenuPage((page) => { }, MockSendSettings);
            RandomizerMenuAPI.AddMenuPage((page) => { }, MockReceiveSettings);

            Log("Initialized");
        }

        private bool MockSendSettings(MenuPage landingPage, out SmallButton button)
        {
            button = new SmallButton(landingPage, "Send Settings");
            button.OnClick += () =>
            {
                //settingsManager?.SaveSettings(dFiler.RootDirectory.CreateDirectory("Preset1"), true, true);

                FileStream fs = File.Create(Path.Combine(presetPath, "Preset1.tar.gz"));
                TgzFiler tf = TgzFiler.CreateForWrite();
                settingsManager?.SaveSettings(tf.RootDirectory, true, true);
                tf.WriteAll(fs);
                fs.Close();

                Log("Sent settings");
            };
            return true;
        }

        private bool MockReceiveSettings(MenuPage landingPage, out SmallButton button)
        {
            button = new SmallButton(landingPage, "Receive Settings");
            button.OnClick += () =>
            {
                //settingsManager?.LoadSettings(dFiler.RootDirectory.CreateDirectory("Preset1"), true);

                FileStream fs = File.OpenRead(Path.Combine(presetPath, "Preset1.tar.gz"));
                TgzFiler tf = TgzFiler.LoadFromStream(fs);
                settingsManager?.LoadSettings(tf.RootDirectory, true);

                Log("Received settings");
            };
            return true;
        }

        public void RegisterConnection<TSettings, TVersion>(RandoSettingsProxy<TSettings, TVersion> settingsProxy)
        {
            settingsManager ??= new SettingsManager();

            settingsManager.Register(settingsProxy.ModKey, new ProxyMetadata(settingsProxy, settingsProxy.VersioningPolicy));
        }

        public void OnLoadGlobal(GlobalSettings s) => GS = s;

        public GlobalSettings OnSaveGlobal() => GS;
    }
}
