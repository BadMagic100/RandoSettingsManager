namespace RandoSettingsManager.Model
{
    public class CreateSettingsInput
    {
        /// <summary>
        /// Base64 encoded settings data
        /// </summary>
        public string? Settings { get; set; }
    }

    public class CreateSettingsOutput
    {
        public string SettingsKey { get; set; }

        public CreateSettingsOutput(string settingsKey)
        {
            SettingsKey = settingsKey;
        }
    }
}
