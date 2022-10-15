﻿using Modding;
using System;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class SimpleSettingsProxy<T> : RandoSettingsProxy<T, string>
    {
        public override string ModKey { get; }
        public override VersioningPolicy<string> VersioningPolicy { get; }

        private readonly Action<T?> receiveSettings;
        private readonly Func<T?>? provideSettings;

        public SimpleSettingsProxy(Mod mod, Action<T?> receiveSettings, Func<T?>? provideSettings = null)
        {
            ModKey = mod.GetName();
            VersioningPolicy = new StrictModVersioningPolicy(mod);
            this.receiveSettings = receiveSettings;
            this.provideSettings = provideSettings;
        }

        public override bool TryProvideSettings(out T? settings)
        {
            if (provideSettings != null)
            {
                settings = provideSettings();
                return settings != null;
            }
            else
            {
                return base.TryProvideSettings(out settings);
            }
        }

        public override void ReceiveSettings(T? settings) => receiveSettings(settings);
    }
}
