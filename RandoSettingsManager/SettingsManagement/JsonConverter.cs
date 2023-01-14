using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Text;

namespace RandoSettingsManager.SettingsManagement
{
    internal class JsonConverter
    {
        private readonly JsonSerializer serializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters =
            {
                new StringEnumConverter(new DefaultNamingStrategy())
            }
        };

        public string Serialize(object? value)
        {
            StringBuilder sb = new();
            StringWriter sw = new(sb);
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jw, value);
            }
            return sb.ToString();
        }

        public T? Deserialize<T>(string value)
        {
            StringReader sr = new(value);
            using (JsonReader jr = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jr);
            }
        }
    }
}
