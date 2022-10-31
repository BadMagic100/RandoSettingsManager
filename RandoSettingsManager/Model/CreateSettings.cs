namespace RandoSettingsManager.Model
{
    internal class CreateSettingsInput
    {
        /// <summary>
        /// Base64 encoded settings data
        /// </summary>
        public string? Settings { get; set; }
    }

    internal class CreateSettingsOutput
    {
        public string SettingsKey { get; set; }

        public CreateSettingsOutput(string settingsKey)
        {
            SettingsKey = settingsKey;
        }
    }
}
