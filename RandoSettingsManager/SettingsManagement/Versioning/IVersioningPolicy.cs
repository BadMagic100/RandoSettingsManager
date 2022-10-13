namespace RandoSettingsManager.SettingsManagement.Versioning
{
    public interface IVersioningPolicy<T>
    {
        public T Version { get; }

        public bool Allow(T version);
    }
}
