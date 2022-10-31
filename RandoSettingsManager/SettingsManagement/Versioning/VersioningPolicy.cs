using Newtonsoft.Json;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A policy to determine what the current version is, and whether a received version is allowable
    /// </summary>
    /// <typeparam name="T">The type used to store version info</typeparam>
    public abstract class VersioningPolicy<T> : ISerializableVersioningPolicy
    {
        /// <summary>
        /// The current version
        /// </summary>
        public abstract T Version { get; }

        /// <summary>
        /// Determines whether the given version is allowed
        /// </summary>
        /// <param name="version">The version to check against the policy</param>
        /// <returns>Whether the given version is allowed</returns>
        public abstract bool Allow(T version);

        string ISerializableVersioningPolicy.SerializedVersion => JsonConvert.SerializeObject(Version);

        bool ISerializableVersioningPolicy.AllowSerialized(string version)
        {
            T? ver = JsonConvert.DeserializeObject<T>(version);
            if (ver == null)
            {
                throw new JsonSerializationException($"Failed to deserialize version for {GetType()}: {version}");
            }
            return Allow(ver);
        }
    }
}
