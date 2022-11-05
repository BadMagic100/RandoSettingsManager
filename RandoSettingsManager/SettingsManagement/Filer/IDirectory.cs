using System.Collections.Generic;

namespace RandoSettingsManager.SettingsManagement.Filer
{
    internal interface IDirectory
    {
        public string Name { get; }
        public IDirectory? Parent { get; }

        public IEnumerable<IDirectory> ListDirectories();
        public IEnumerable<IFile> ListFiles();

        public IDirectory? GetDirectory(string name);
        public IFile? GetFile(string name);

        public IFile CreateFile(string name);
        public IDirectory CreateDirectory(string name);

        public void Delete(bool recursive);
        public void Clear(bool recursiveDeletes);
    }
}
