using System.IO;

namespace RandoSettingsManager.SettingsManagement.Filer.Disk
{
    internal class DiskFile : IFile
    {
        readonly FileInfo f;

        public string Name { get; }

        public IDirectory Parent { get; }

        public DiskFile(FileInfo f)
        {
            Parent = new DiskDirectory(f.Directory);
            Name = f.Name;
            this.f = f;
        }

        public DiskFile(DirectoryInfo parent, string name)
        {
            Parent = new DiskDirectory(parent);
            Name = name;
            f = new FileInfo(Path.Combine(parent.FullName, name));
        }

        public string ReadContent()
        {
            if (!f.Exists)
            {
                return "";
            }
            using StreamReader sr = f.OpenText();
            return sr.ReadToEnd();
        }

        public void WriteContent(string content)
        {
            using StreamWriter sw = f.CreateText();
            sw.Write(content);
        }

        public void Delete()
        {
            f.Delete();
        }
    }
}
