using System.Net;
using System.Text;

namespace RandoSettingsManager.Model
{
    public class RetrieveSettingsInput
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

    public class RetrieveSettingsOutput
    {
        public bool Found { get; set; } = false;

        /// <summary>
        /// Base64 encoded settings data
        /// </summary>
        public string? Settings { get; set; }
    }
}
