using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
