using Modding;
using RandoSettingsManager.SettingsManagement.Versioning;
using System;

namespace RandoSettingsManager.SettingsManagement
{
    /// <summary>
    /// A simple settings proxy implementing strict versioning and standard naming
    /// </summary>
    /// <typeparam name="T">The type to use to store settings</typeparam>
    public class SimpleSettingsProxy<T> : RandoSettingsProxy<T, string>
    {
        /// <inheritdoc/>
        public override string ModKey { get; }
        /// <inheritdoc/>
        public override VersioningPolicy<string> VersioningPolicy { get; }

        private readonly Action<T?> receiveSettings;
        private readonly Func<T?>? provideSettings;

        /// <summary>
        /// Constructs a SimpleSettingsProxy
        /// </summary>
        /// <param name="mod">The mod to use for naming and versioning</param>
        /// <param name="receiveSettings">A handler for settings to be received. Implements <see cref="ReceiveSettings(T?)"/></param>
        /// <param name="provideSettings">A handler for settings to be provided. Implements <see cref="TryProvideSettings(out T?)"/></param>
        public SimpleSettingsProxy(Mod mod, Action<T?> receiveSettings, Func<T?>? provideSettings = null)
        {
            ModKey = mod.GetName();
            VersioningPolicy = new StrictModVersioningPolicy(mod);
            this.receiveSettings = receiveSettings;
            this.provideSettings = provideSettings;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void ReceiveSettings(T? settings) => receiveSettings(settings);
    }
}
