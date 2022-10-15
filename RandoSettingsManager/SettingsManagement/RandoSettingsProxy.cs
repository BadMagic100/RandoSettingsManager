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
                // don't throw here - there are several reasons why settings may fail to deserialize under normal circumstances, such as
                // mods changing their settings structure in contexts where versioning rules are ignored
                RandoSettingsManagerMod.Instance.LogError($"Failed to deserialize settings for {GetType()}: {settings}");
            }
            else
            {
                ReceiveSettings(s);
            }
        }
    }
}
