﻿using ICSharpCode.SharpZipLib.Tar;
using System.Collections.Generic;
using System.IO;

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
                string dirName = Path.GetDirectoryName(name);
                TarDirectory parentDir = dirs[dirName];
                if (entry.IsDirectory)
                {
                    TarDirectory childDir = new(parentDir, entry);
                    parentDir.dirs[childDir.Name] = childDir;
                }
                else
                {
                    TarFile childFile = new(parentDir, entry, tar);
                    parentDir.files[childFile.Name] = childFile;
                }
            }
            return root;
        }
    }
}