using System.Net;
using System.Text;

namespace RandoSettingsManager.Model
{
    internal class RetrieveSettingsInput
    {
        public string? SettingsKey { get; set; }

        public string ToQueryString()
        {
            StringBuilder sb = new("?");

            sb.Append(nameof(SettingsKey));
            sb.Append('=');
            sb.Append(WebUtility.UrlEncode(SettingsKey));

            return sb.ToString();
        }
    }

    internal class RetrieveSettingsOutput
    {
        public bool Found { get; set; } = false;

        /// <summary>
        /// Base64 encoded settings data
        /// </summary>
        public string? Settings { get; set; }
    }
}
