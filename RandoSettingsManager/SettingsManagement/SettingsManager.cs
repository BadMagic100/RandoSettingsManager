using System;
using System.Collections.Generic;
using System.Reflection;

namespace RandoSettingsManager.SettingsManagement
{
    internal record ProxyMetadata(Type GenericProxyType, Type SettingsType, Type VersionType, 
        object VersionPolicy, bool CanProvideSettings, object Proxy);

    internal class SettingsManager
    {
        static MethodInfo GetProvideSettings(ProxyMetadata md) => md.GenericProxyType
            .GetMethod(nameof(RandoSettingsProxy<object, object>.ProvideSettings));

        static readonly MethodInfo receiveSettings = typeof(RandoSettingsProxy<,>)
            .GetMethod(nameof(RandoSettingsProxy<object, object>.ReceiveSettings));

        readonly Dictionary<string, ProxyMetadata> metadata = new();

        public void Register(string key, ProxyMetadata md)
        {
            metadata[key] = md;
        }

        public void SaveSettings()
        {

        }

        public void LoadSettings()
        {

        }
    }
}
