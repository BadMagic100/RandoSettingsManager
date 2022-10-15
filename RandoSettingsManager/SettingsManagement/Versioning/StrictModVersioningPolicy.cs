using Modding;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class StrictModVersioningPolicy : VersioningPolicy<string>
    {
        public override string Version { get; }

        public StrictModVersioningPolicy(Mod mod)
        {
            Version = mod.GetVersion();
        }

        public override bool Allow(string version) => Version == version;
    }
}
