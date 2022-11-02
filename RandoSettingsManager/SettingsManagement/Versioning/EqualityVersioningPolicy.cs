using System.Collections.Generic;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy that provides an arbitrary dev-provided version, and checks equality with
    /// that version to allow incoming versions
    /// </summary>
    /// <typeparam name="T">The version type</typeparam>
    public class EqualityVersioningPolicy<T> : VersioningPolicy<T>
    {
        private readonly IComparer<T> comparer;

        /// <inheritdoc/>
        public override T Version { get; }

        /// <summary>
        /// Constructs a policy with the default comparer
        /// </summary>
        /// <param name="version">The local version to provide and compare against</param>
        public EqualityVersioningPolicy(T version) : this(version, Comparer<T>.Default) { }

        /// <summary>
        /// Constructs a policy with a custom comparer
        /// </summary>
        /// <param name="version"></param>
        /// <param name="comparer"></param>
        public EqualityVersioningPolicy(T version, IComparer<T> comparer)
        {
            Version = version;
            this.comparer = comparer;
        }

        /// <inheritdoc/>
        public override bool Allow(T version) => comparer.Compare(version, Version) == 0;
    }
}
