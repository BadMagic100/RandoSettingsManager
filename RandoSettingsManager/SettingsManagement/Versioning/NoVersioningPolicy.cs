namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class NoVersioningPolicy : VersioningPolicy<int>
    {
        public override int Version => 0;

        public override bool Allow(int version) => true;
    }
}
