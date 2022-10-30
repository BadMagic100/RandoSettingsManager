using Modding;
using RandoSettingsManager.Menu;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.Testing;
using System;

namespace RandoSettingsManager
{
    public class RandoSettingsManagerMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal SettingsManager? settingsManager;
        private static RandoSettingsManagerMod? _instance;

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
            SettingsMenu.HookMenu();
            RegisterConnection(new TestSettingsProxy());

            Log("Initialized");
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
