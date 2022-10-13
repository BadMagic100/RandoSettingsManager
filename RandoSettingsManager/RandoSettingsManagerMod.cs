using Modding;
using RandoSettingsManager.SettingsManagement;
using System;

namespace RandoSettingsManager
{
    public class RandoSettingsManagerMod : Mod
    {
        internal SettingsManager? settingsManager;
        private static RandoSettingsManagerMod? _instance;

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

            Log("Initialized");
        }

        public void RegisterConnection<TSettings, TVersion>(RandoSettingsProxy<TSettings, TVersion> settingsProxy)
        {
            settingsManager ??= new SettingsManager();

            Type rootProxyType = settingsProxy.GetType();
            while (rootProxyType.GetGenericTypeDefinition() != typeof(RandoSettingsProxy<,>))
            {
                rootProxyType = rootProxyType.BaseType;
            }

            Type settingsType = rootProxyType.GenericTypeArguments[0];
            Type versionType = rootProxyType.GenericTypeArguments[1];

            settingsManager.Register(settingsProxy.ModKey, 
                new ProxyMetadata(rootProxyType, settingsType, versionType, 
                                  settingsProxy.VersioningPolicy, settingsProxy.CanProvideSettings, 
                                  settingsProxy));
        }
    }
}
