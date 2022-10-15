using Newtonsoft.Json;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace RandoSettingsManager.SettingsManagement
{
    public abstract class RandoSettingsProxy<TSettings, TVersion> : ISerializableSettingsProxy
    {
        public abstract string ModKey { get; }

        public abstract VersioningPolicy<TVersion> VersioningPolicy { get; }

        public virtual bool TryProvideSettings(out TSettings? settings)
        {
            settings = default;
            return false;
        }

        public abstract void ReceiveSettings(TSettings? settings);

        bool ISerializableSettingsProxy.TryProvideSerializedSettings(out string? settings)
        {
            if (TryProvideSettings(out TSettings? s))
            {
                settings = JsonConvert.SerializeObject(s);
                return true;
            }
            else
            {
                settings = null;
                return false;
            }
        }

        void ISerializableSettingsProxy.ReceiveSerializedSettings(string? settings)
        {
            if (settings == null)
            {
                ReceiveSettings(default);
                return;
            }
            
            TSettings? s = JsonConvert.DeserializeObject<TSettings>(settings);
            if (s == null)
            {
                throw new JsonSerializationException($"Failed to deserialize settings for {GetType()}: {settings}");
            }
            ReceiveSettings(s);
        }
    }
}
