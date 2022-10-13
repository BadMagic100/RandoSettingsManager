using Modding;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class StrictModVersioningPolicy : IVersioningPolicy<string>
    {
        public string Version { get; }

        public StrictModVersioningPolicy(Mod mod)
        {
            Version = mod.GetVersion();
        }

        public bool Allow(string version) => Version == version;
    }
}
