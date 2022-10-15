using System;

namespace RandoSettingsManager.SettingsManagement
{
    internal class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
