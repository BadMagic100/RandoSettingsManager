using RandoSettingsManager.SettingsManagement.Versioning;
using System;

namespace RandoSettingsManager.SettingsManagement
{
    /// <summary>
    /// The base class for settings proxies which implement handlers for settings events
    /// and versioning information
    /// </summary>
    /// <typeparam name="TSettings">The type used to store settings</typeparam>
    /// <typeparam name="TVersion">The type used to store version information</typeparam>
    public abstract class RandoSettingsProxy<TSettings, TVersion> : ISerializableSettingsProxy
    {
        /// <summary>
        /// The unique key to identify this proxy (usually the mod name)
        /// </summary>
        public abstract string ModKey { get; }

        /// <summary>
        /// The versioning policy to accept and reject settings
        /// </summary>
        /// <remarks>
        /// Generally, versioning policies cannot safely change type, and can usually
        /// only be safely changed to be less strict. Generally you'll be best to just
        /// pick one and stick with it, so think carefully about the guarantees you
        /// want to make so you don't have to change it in the future.
        /// </remarks>
        public abstract VersioningPolicy<TVersion> VersioningPolicy { get; }

        /// <summary>
        /// Attempts to provide settings when sending/saving data
        /// </summary>
        /// <param name="settings">The settings to be sent/saved</param>
        /// <returns>True if settings should be sent, otherwise false</returns>
        public virtual bool TryProvideSettings(out TSettings? settings)
        {
            settings = default;
            return false;
        }

        /// <summary>
        /// Handles receiving settings, or disabling the connection if no
        /// settings were received for this connection. Connections may perform their
        /// own validation and throw <see cref="ValidationException"/> to indicate validation
        /// failures.
        /// </summary>
        /// <param name="settings">The received settings, if any</param>
        public abstract void ReceiveSettings(TSettings? settings);

        bool ISerializableSettingsProxy.TryProvideSerializedSettings(JsonConverter jsonConverter, out string? settings)
        {
            if (TryProvideSettings(out TSettings? s))
            {
                settings = jsonConverter.Serialize(s);
                return true;
            }
            else
            {
                settings = null;
                return false;
            }
        }

        void ISerializableSettingsProxy.ReceiveSerializedSettings(JsonConverter jsonConverter, string? settings)
        {
            if (settings == null)
            {
                ReceiveSettings(default);
                return;
            }

            TSettings? s;
            try
            {
                s = jsonConverter.Deserialize<TSettings>(settings);
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.LogError($"Failed to deserialize settings for {GetType()}: {settings}");
                RandoSettingsManagerMod.Instance.LogError(ex);
                throw new ValidationException($"Failed to deserialize settings for {ModKey}.");
            }

            if (s == null)
            {
                RandoSettingsManagerMod.Instance.LogError($"Failed to deserialize settings for {GetType()}: {settings}");
                throw new ValidationException($"Failed to deserialize settings for {ModKey}.");
            }
            else
            {
                ReceiveSettings(s);
            }
        }
    }
}
