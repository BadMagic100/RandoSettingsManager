using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RandoSettingsManager
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SettingsManagementMode
    {
        Classic,
        Modern
    }

    public class GlobalSettings
    {
        public SettingsManagementMode Mode { get; set; } = SettingsManagementMode.Modern;

        public bool HasImportedProfiles { get; set; } = false;
    }
}
