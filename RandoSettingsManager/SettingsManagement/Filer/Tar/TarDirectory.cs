using ICSharpCode.SharpZipLib.Tar;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandoSettingsManager.SettingsManagement.Filer.Tar
{
    internal class TarDirectory : IDirectory
    {
        public string Name { get; }

        public IDirectory? Parent { get; }

        private Dictionary<string, TarDirectory> dirs = new();
        private Dictionary<string, TarFile> files = new();

        /// <summary>
        /// Creates the root tar directory
        /// </summary>
        public TarDirectory()
        {
            Name = string.Empty;
            Parent = null;
        }

        public TarDirectory(TarDirectory dir, string name)
        {
            Name = name;
            Parent = dir;
        }

        public TarDirectory(TarDirectory dir, TarEntry entry)
        {
            string dirName = Path.GetDirectoryName(entry.Name).Replace('\\', '/');
            Name = Path.GetFileName(dirName);
            Parent = dir;
        }

        public IDirectory CreateDirectory(string name)
        {
            if (!dirs.ContainsKey(name))
            {
                dirs[name] = new TarDirectory(this, name);
            }
            return dirs[name];
        }

        public IFile CreateFile(string name)
        {
            files[name] = new TarFile(this, name);
            return files[name];
        }

        public IDirectory? GetDirectory(string name) => dirs.ContainsKey(name) ? dirs[name] : null;

        public IFile? GetFile(string name) => files.ContainsKey(name) ? files[name] : null;

        public IEnumerable<IDirectory> ListDirectories() => dirs.Values;

        public IEnumerable<IFile> ListFiles() => files.Values;

        internal void ReleaseSubdirectory(string name)
        {
            dirs.Remove(name);
        }

        internal void ReleaseFile(string name)
        {
            files.Remove(name);
        }

        public void Delete(bool recursive)
        {
            if (!recursive && (dirs.Any() || files.Any()))
            {
                throw new IOException("Cannot delete a non-empty directory non-recursively");
            }

            Clear(recursive);
            (Parent as TarDirectory)?.ReleaseSubdirectory(Name);
        }

        public void Clear(bool recursiveDeletes)
        {
            foreach (IDirectory d in dirs.Values)
            {
                d.Delete(recursiveDeletes);
            }
            foreach (IFile f in files.Values)
            {
                f.Delete();
            }
            dirs.Clear();
            files.Clear();
        }

        public void Persist(TarOutputStream target)
        {
            // don't create an entry for the root dir
            if (Parent != null)
            {
                target.PutNextEntry(TarEntry.CreateTarEntry(this.TarPath()));
            }
            foreach (TarDirectory dir in dirs.Values)
            {
                dir.Persist(target);
            }
            foreach (TarFile file in files.Values)
            {
                file.Persist(target);
            }
        }

        public static TarDirectory Load(TarInputStream tar)
        {
            TarDirectory root = new();
            Dictionary<string, TarDirectory> dirs = new()
            {
                [""] = root
            };

            TarEntry? entry = tar.GetNextEntry();
            while(entry != null)
            {
                string name = entry.Name.TrimStart('/');
                RandoSettingsManagerMod.Instance.LogDebug($"Loading entry {name}");
                if (entry.IsDirectory)
                {
                    string dirName = Path.GetDirectoryName(name);
                    string parentDirName = Path.GetDirectoryName(dirName);
                    RandoSettingsManagerMod.Instance.LogDebug($"Looking for parent directory {parentDirName}");
                    TarDirectory parentDir = dirs[parentDirName];
                    TarDirectory childDir = new(parentDir, entry);
                    dirs[dirName] = childDir;
                    parentDir.dirs[childDir.Name] = childDir;
                }
                else
                {
                    string dirName = Path.GetDirectoryName(name);
                    RandoSettingsManagerMod.Instance.LogDebug($"Looking for parent directory {dirName}");
                    TarDirectory parentDir = dirs[dirName];
                    TarFile childFile = new(parentDir, entry, tar);
                    parentDir.files[childFile.Name] = childFile;
                }
                entry = tar.GetNextEntry();
            }
            return root;
        }
    }
}
