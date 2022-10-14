using System;
using System.Collections.Generic;
using System.Reflection;

namespace RandoSettingsManager.SettingsManagement
{
    internal record ProxyMetadata(Type GenericProxyType, Type SettingsType, Type VersionType, 
        object VersionPolicy, object Proxy);

    internal class SettingsManager
    {
        static MethodInfo GetTryProvideSettings(ProxyMetadata md) => md.GenericProxyType
            .GetMethod(nameof(RandoSettingsProxy<object, object>.TryProvideSettings));

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
