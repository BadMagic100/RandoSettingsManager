namespace RandoSettingsManager.SettingsManagement
{
    internal interface ISerializableSettingsProxy
    {
        internal bool TryProvideSerializedSettings(out string? settings);

        internal void ReceiveSerializedSettings(string? settings);
    }
}
