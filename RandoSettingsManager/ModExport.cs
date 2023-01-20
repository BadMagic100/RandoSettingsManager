using Modding;
using MonoMod.ModInterop;
using RandoSettingsManager.SettingsManagement;
using System;

namespace RandoSettingsManager
{
    /// <summary>
    /// Class which exports parts of the RandoSettingsManager API for use in mod interop.
    /// </summary>
    [ModExportName("RandoSettingsManager")]
    internal static class ModExport
    {
        /// <summary>
        /// Registers a connection mod for settings events with a <see cref="SimpleSettingsProxy{T}"/>.
        /// </summary>
        /// <param name="mod">The mod to use for naming and versioning</param>
        /// <param name="settingsType">The type used to store settings</param>
        /// <param name="receiveSettings">An <see cref="Action{settingsType}"/> handler for settings to be received. Used to implement <see cref="RandoSettingsProxy{TSettings, TVersion}.ReceiveSettings"/></param>
        /// <param name="provideSettings">An optional <see cref="Func{settingType}"/> handler for settings to be provided. Used to implement <see cref="RandoSettingsProxy{TSettings, TVersion}.TryProvideSettings"/></param>
        public static void RegisterConnectionSimple(Mod mod, Type settingsType, Delegate receiveSettings, Delegate? provideSettings)
        {
            try
            {
                Type proxyType = typeof(SimpleSettingsProxy<>).MakeGenericType(settingsType);
                object proxy = Activator.CreateInstance(proxyType, new object?[] { mod, receiveSettings, provideSettings });
                typeof(RandoSettingsManagerMod).GetMethod(nameof(RandoSettingsManagerMod.RegisterConnection))
                    .MakeGenericMethod(settingsType, typeof(string))
                    .Invoke(RandoSettingsManagerMod.Instance, new object[] { proxy });
            }
            catch (MissingMethodException mme)
            {
                if (!typeof(Action<>).MakeGenericType(settingsType).IsAssignableFrom(receiveSettings.GetType()))
                {
                    throw new ArgumentException($"Failed to register connection {mod?.Name} through ModInterop: parameter {nameof(receiveSettings)} must convert to Action<{settingsType.Name}>");
                }
                if (provideSettings is not null && !typeof(Func<>).MakeGenericType(settingsType).IsAssignableFrom(provideSettings.GetType()))
                {
                    throw new ArgumentException($"Failed to register connection {mod?.Name} through ModInterop: parameter {nameof(provideSettings)} must convert to Func<{settingsType.Name}>");
                }
                throw new InvalidOperationException($"Failed to register connection {mod?.Name} through ModInterop", mme);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to register connection {mod?.Name} through ModInterop", e);
            }
        }
    }
}
