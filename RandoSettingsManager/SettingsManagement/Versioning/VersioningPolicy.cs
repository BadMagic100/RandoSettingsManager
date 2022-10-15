using Newtonsoft.Json;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public abstract class VersioningPolicy<T> : ISerializableVersioningPolicy
    {
        public abstract T Version { get; }

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
