using RandoSettingsManager.SettingsManagement.Versioning;

namespace RandoSettingsManager.SettingsManagement
{
    public abstract class RandoSettingsProxy<TSettings, TVersion>
    {
        public abstract string ModKey { get; }

        public abstract IVersioningPolicy<TVersion> VersioningPolicy { get; }

        public virtual bool TryProvideSettings(out TSettings? settings)
        {
            settings = default;
            return false;
        }

        public abstract void ReceiveSettings(TSettings? settings);
    }
}
