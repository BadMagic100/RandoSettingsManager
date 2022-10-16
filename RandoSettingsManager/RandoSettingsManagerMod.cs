using MenuChanger;
using MenuChanger.MenuElements;
using Modding;
using RandomizerMod.Menu;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Disk;
using RandoSettingsManager.Testing;
using System;
using System.IO;
using UnityEngine;

namespace RandoSettingsManager
{
    public class RandoSettingsManagerMod : Mod
    {
        internal SettingsManager? settingsManager;
        private static RandoSettingsManagerMod? _instance;

        private static DiskFiler dFiler = new(Path.Combine(Application.persistentDataPath, "Randomizer 4", "Presets"));

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
                settingsManager?.SaveSettings(dFiler.RootDirectory.CreateDirectory("Preset1"), true, true);
                Log("Sent settings");
            };
            return true;
        }

        private bool MockReceiveSettings(MenuPage landingPage, out SmallButton button)
        {
            button = new SmallButton(landingPage, "Receive Settings");
            button.OnClick += () =>
            {
                settingsManager?.LoadSettings(dFiler.RootDirectory.CreateDirectory("Preset1"), true);
                Log("Received settings");
            };
            return true;
        }

        public void RegisterConnection<TSettings, TVersion>(RandoSettingsProxy<TSettings, TVersion> settingsProxy)
        {
            settingsManager ??= new SettingsManager();

            settingsManager.Register(settingsProxy.ModKey, new ProxyMetadata(settingsProxy, settingsProxy.VersioningPolicy));
        }
    }
}
