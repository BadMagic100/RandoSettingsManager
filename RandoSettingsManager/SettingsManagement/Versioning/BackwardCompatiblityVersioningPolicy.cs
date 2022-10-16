using System.Collections.Generic;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class BackwardCompatiblityVersioningPolicy<T> : VersioningPolicy<T>
    {
        private readonly Comparer<T> versionComparer;

        public override T Version { get; }

        public BackwardCompatiblityVersioningPolicy(T targetVersion, Comparer<T> versionComparer)
        {
            // if I claim to be backwards compatible with version X, then my version is effectively X
            // regardless of my actual version
            Version = targetVersion;
            this.versionComparer = versionComparer;
        }   

        public override bool Allow(T version)
        {
            // allow the settings if the provided version (version) is at least as new as
            // the target version (Version)
            return versionComparer.Compare(version, Version) != -1;
        }
    }
}
