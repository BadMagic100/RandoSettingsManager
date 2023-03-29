using System;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy that performs no version checking. Using this versioning policy
    /// implies indefinite backwards compatibility of settings and logic (failing to meet this promise
    /// may cause settings to fail to load or hash mismatches).
    /// </summary>
    [Obsolete("This versioning policy is not recommended for most used cases. " +
        "Consider StrictModVersioningPolicy if you're just looking for a simple-to-use policy.")]
    public class NoVersioningPolicy : VersioningPolicy<object?>
    {
        /// <inheritdoc/>
        public override object? Version => null;

        /// <inheritdoc/>
        protected override bool AllowsNullValues => true;

        /// <inheritdoc/>
        public override bool Allow(object? version) => true;
    }
}
