using RandoSettingsManager.SettingsManagement.Versioning;
using System.Collections.Generic;

namespace RandoSettingsManager.SettingsManagement
{
    internal record ProxyMetadata(ISerializableSettingsProxy Proxy, ISerializableVersioningPolicy VersioningPolicy);

    internal class SettingsManager
    {
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
