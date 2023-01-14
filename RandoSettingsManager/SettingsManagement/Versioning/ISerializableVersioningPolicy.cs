namespace RandoSettingsManager.SettingsManagement.Versioning
{
    internal interface ISerializableVersioningPolicy
    {
        internal string GetSerializedVersion(JsonConverter jsonConverter);
        internal bool AllowSerialized(JsonConverter jsonConverter, string version);
    }
}
