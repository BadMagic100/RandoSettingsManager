using System;
using System.Collections.Generic;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy which guarantees backwards compatibility until a given target version.
    /// </summary>
    /// <typeparam name="T">The type to store version</typeparam>
    [Obsolete("Prefer EqualityVersioningPolicy instead, it's the same but named better")]
    public class BackwardCompatiblityVersioningPolicy<T> : VersioningPolicy<T>
    {
        private readonly Comparer<T> versionComparer;

        /// <inheritdoc/>
        public override T Version { get; }

        /// <summary>
        /// Constructs a backwards compatibility version policy using the default comparer
        /// </summary>
        /// <param name="targetVersion">The minimum target version for backwards compatibility</param>
        public BackwardCompatiblityVersioningPolicy(T targetVersion) : this(targetVersion, Comparer<T>.Default) { }

        /// <summary>
        /// Constructs a backwards compatibility version policy using the specified comparer
        /// </summary>
        /// <param name="targetVersion">The minimum target version for backwards compatibility</param>
        /// <param name="versionComparer">The comparer to use for comparison</param>
        public BackwardCompatiblityVersioningPolicy(T targetVersion, Comparer<T> versionComparer)
        {
            // if I claim to be backwards compatible with version X, then my version is effectively X
            // regardless of my actual version
            Version = targetVersion;
            this.versionComparer = versionComparer;
        }   

        /// <inheritdoc/>
        public override bool Allow(T version)
        {
            // allow the settings if the provided version (version) is the same as the
            // target version (Version). Compatible versions in the future are "rolled back"
            // in the constructor
            return versionComparer.Compare(version, Version) == 0;
        }
    }
}
