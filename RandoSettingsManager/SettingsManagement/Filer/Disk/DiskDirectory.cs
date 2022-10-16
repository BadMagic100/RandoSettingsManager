using System.Collections.Generic;
using System.IO;

namespace RandoSettingsManager.SettingsManagement.Filer.Disk
{
    internal class DiskDirectory : IDirectory
    {
        readonly DirectoryInfo dir;

        public string Name { get; }

        public IDirectory? Parent { get; }

        public DiskDirectory(DirectoryInfo dir)
        {
            this.dir = dir;
            Name = dir.Name;
            if (dir.Parent != null)
            {
                Parent = new DiskDirectory(dir.Parent);
            }
            if (!dir.Exists)
            {
                dir.Create();
            }
        }

        public IDirectory CreateDirectory(string name)
        {
            DirectoryInfo d = dir.CreateSubdirectory(name);
            return new DiskDirectory(d);
        }

        public IFile CreateFile(string name)
        {
            return new DiskFile(dir, name);
        }

        public IDirectory? GetDirectory(string name)
        {
            DirectoryInfo[] found = dir.GetDirectories(name);
            if (found.Length > 0)
            {
                return new DiskDirectory(found[0]);
            }
            return null;
        }

        public IFile? GetFile(string name)
        {
            FileInfo[] found = dir.GetFiles(name);
            if (found.Length > 0)
            {
                return new DiskFile(found[0]);
            }
            return null;
        }

        public IEnumerable<IDirectory> ListDirectories()
        {
            foreach (DirectoryInfo d in dir.EnumerateDirectories())
            {
                yield return new DiskDirectory(d);
            }
        }

        public IEnumerable<IFile> ListFiles()
        {
            foreach (FileInfo f in dir.EnumerateFiles())
            {
                yield return new DiskFile(f);
            }
        }
    }
}
