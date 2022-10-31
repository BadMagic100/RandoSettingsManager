using Modding;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy that uses and strictly compares against the version of a <see cref="Mod"/> instance.
    /// </summary>
    public class StrictModVersioningPolicy : VersioningPolicy<string>
    {
        /// <inheritdoc/>
        public override string Version { get; }

        /// <summary>
        /// Constructs a StrictModVersioningPolicy
        /// </summary>
        /// <param name="mod">The mod instance to get the version from</param>
        public StrictModVersioningPolicy(Mod mod)
        {
            Version = mod.GetVersion();
        }

        /// <inheritdoc/>
        public override bool Allow(string version) => Version == version;
    }
}
