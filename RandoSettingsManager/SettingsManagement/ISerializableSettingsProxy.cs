namespace RandoSettingsManager.SettingsManagement
{
    internal interface ISerializableSettingsProxy
    {
        internal bool TryProvideSerializedSettings(JsonConverter jsonConverter, out string? settings);

        internal void ReceiveSerializedSettings(JsonConverter jsonConverter, string? settings);
    }
}
