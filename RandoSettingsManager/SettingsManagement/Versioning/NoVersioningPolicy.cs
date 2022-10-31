namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy that performs no version checking. Using this versioning policy
    /// implies indefinite backwards compatibility of settings (failing to meet this promise
    /// may cause settings to fail to load).
    /// </summary>
    public class NoVersioningPolicy : VersioningPolicy<int>
    {
        /// <inheritdoc/>
        public override int Version => 0;

        /// <inheritdoc/>
        public override bool Allow(int version) => true;
    }
}
