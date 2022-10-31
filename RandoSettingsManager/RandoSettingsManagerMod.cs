using Modding;
using RandomizerMod.Settings;
using RandoSettingsManager.Menu;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Filer.Disk;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace RandoSettingsManager
{
    /// <summary>
    /// Mod class for RandoSettingsManager which provides an API interface for connections to hook into settings events
    /// </summary>
    public class RandoSettingsManagerMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal SettingsManager settingsManager = new();
        private static RandoSettingsManagerMod? _instance;

        internal GlobalSettings GS { get; private set; } = new();

        /// <summary>
        /// Singleton instance of the mod
        /// </summary>
        public static RandoSettingsManagerMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(RandoSettingsManagerMod)} was never constructed");
                }
                return _instance;
            }
        }

        /// <inheritdoc/>
        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        /// <inheritdoc/>
        public RandoSettingsManagerMod() : base("RandoSettingsManager")
        {
            _instance = this;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Log("Initializing");

            // create menus and such
            SettingsMenu.HookMenu();
            // auto-import profiles from rando4's profile management
            if (!GS.HasImportedProfiles)
            {
                foreach (MenuProfile prof in RandomizerMod.RandomizerMod.GS.Profiles)
                {
                    if (prof == null || prof.name == null || prof.settings == null)
                    {
                        continue;
                    }

                    StringBuilder b = new();
                    foreach (char c in prof.name)
                    {
                        if (!Path.GetInvalidFileNameChars().Contains(c))
                        {
                            b.Append(c);
                        }
                    }
                    string filteredName = b.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(filteredName))
                    {
                        string path = Path.Combine(SettingsMenu.ProfilesDir, filteredName);
                        settingsManager.WriteRandoProfile(new DiskFiler(path).RootDirectory, 
                            false, false, prof.settings);
                    }
                }
                GS.HasImportedProfiles = true;
            }

            Log("Initialized");
        }

        /// <summary>
        /// Registers a connection mod for settings events
        /// </summary>
        /// <typeparam name="TSettings">The type used to store settings</typeparam>
        /// <typeparam name="TVersion">The type used to store version information</typeparam>
        /// <param name="settingsProxy">The settings proxy hosting event handlers and version information</param>
        public void RegisterConnection<TSettings, TVersion>(RandoSettingsProxy<TSettings, TVersion> settingsProxy)
        {
            settingsManager.Register(settingsProxy.ModKey, new ProxyMetadata(settingsProxy, settingsProxy.VersioningPolicy));
        }

        void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings s) => GS = s;

        GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal() => GS;
    }
}
