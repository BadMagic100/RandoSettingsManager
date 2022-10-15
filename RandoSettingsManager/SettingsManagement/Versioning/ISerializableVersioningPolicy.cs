namespace RandoSettingsManager.SettingsManagement.Versioning
{
    internal interface ISerializableVersioningPolicy
    {
        internal string SerializedVersion { get; }
        internal bool AllowSerialized(string version);
    }
}
