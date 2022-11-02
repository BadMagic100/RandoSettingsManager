namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy that performs no version checking. Using this versioning policy
    /// implies indefinite backwards compatibility of settings and logic (failing to meet this promise
    /// may cause settings to fail to load or hash mismatches).
    /// </summary>
    public class NoVersioningPolicy : VersioningPolicy<object?>
    {
        /// <inheritdoc/>
        public override object? Version => null;

        /// <inheritdoc/>
        public override bool Allow(object? version) => true;
    }
}
