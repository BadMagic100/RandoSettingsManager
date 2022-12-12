using System;

namespace RandoSettingsManager.SettingsManagement
{
    /// <summary>
    /// An exception which indicates a validation failure during settings loading.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <inheritdoc/>
        public ValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// An exception which indicates a late validation failure, after settings have been partially loaded.
    /// Used by SettingsManager to distinguish between validation failures before and after settings have
    /// been applied.
    /// </summary>
    internal class LateValidationException : Exception
    {
        public LateValidationException(string message) : base(message) { }
    }
}
