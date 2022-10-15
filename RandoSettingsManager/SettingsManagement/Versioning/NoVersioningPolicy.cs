namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public class NoVersioningPolicy : IVersioningPolicy<int>
    {
        public int Version => 0;

        public bool Allow(int version) => true;
    }
}
