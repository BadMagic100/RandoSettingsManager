using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using RandoSettingsManager.SettingsManagement.Versioning.Comparators;

namespace RandoSettingsManager.Testing
{
    public record TestConnection(string Text, int Number);

    internal class TestSettingsProxy : RandoSettingsProxy<TestConnection, string>
    {
        public override string ModKey => "RandoSettingsManager.Test1";

        public override VersioningPolicy<string> VersioningPolicy { get; } = new BackwardCompatiblityVersioningPolicy<string>("0.1", new SemVerComparator());

        public override void ReceiveSettings(TestConnection? settings)
        {
            RandoSettingsManagerMod.Instance.Log($"Received settings: {settings}");
        }

        public override bool TryProvideSettings(out TestConnection? settings)
        {
            settings = new TestConnection("Hello", 5);
            return true;
        }
    }
}
