using Modding;
using System;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class SimpleSettingsProxy<T> : RandoSettingsProxy<T, string>
    {
        public override string ModKey { get; }
        public override IVersioningPolicy<string> VersioningPolicy { get; }
        public override bool CanProvideSettings => provideSettings != null;

        private readonly Action<T?> receiveSettings;
        private readonly Func<T>? provideSettings;

        public SimpleSettingsProxy(Mod mod, Action<T?> receiveSettings, Func<T>? provideSettings = null)
        {
            ModKey = mod.GetName();
            VersioningPolicy = new StrictModVersioningPolicy(mod);
            this.receiveSettings = receiveSettings;
            this.provideSettings = provideSettings;
        }

        public override T? ProvideSettings() => provideSettings != null ? provideSettings.Invoke() : base.ProvideSettings();

        public override void ReceiveSettings(T? settings) => receiveSettings(settings);
    }
}
