using System.IO;

namespace RandoSettingsManager.SettingsManagement.Filer.Disk
{
    internal class DiskFiler
    {
        public IDirectory RootDirectory { get; }

        public DiskFiler(string rootDir)
        {
            RootDirectory = new DiskDirectory(new DirectoryInfo(rootDir));
        }
    }
}
