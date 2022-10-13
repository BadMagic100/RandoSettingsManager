using RandoSettingsManager.SettingsManagement.Versioning;
using System;

namespace RandoSettingsManager.SettingsManagement
{
    public abstract class RandoSettingsProxy<TSettings, TVersion>
    {
        public abstract string ModKey { get; }

        public abstract IVersioningPolicy<TVersion> VersioningPolicy { get; }

        public abstract bool CanProvideSettings { get; }

        public virtual TSettings? ProvideSettings()
        {
            throw new NotImplementedException($"{GetType().Name} does not support saving/sharing settings");
        }

        public abstract void ReceiveSettings(TSettings? settings);
    }
}
