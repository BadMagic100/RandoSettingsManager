using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using System.Text;

namespace RandoSettingsManager.SettingsManagement.Filer.Tar
{
    internal class TarFile : IFile
    {
        public string Name { get; }

        public IDirectory Parent { get; }

        private string content;

        public TarFile(TarDirectory dir, string name)
        {
            Name = name;
            Parent = dir;
            content = "";
        }

        public TarFile(TarDirectory dir, TarEntry entry, TarInputStream contentStream)
        {
            Name = Path.GetFileName(entry.Name);
            Parent = dir;
            byte[] readBuffer = new byte[entry.Size];
            using MemoryStream target = new();

            int numRead = contentStream.Read(readBuffer, 0, readBuffer.Length);
            while (numRead > 0)
            {
                target.Write(readBuffer, 0, numRead);
                numRead = contentStream.Read(readBuffer, 0, readBuffer.Length);
            }

            byte[] byteContent = target.ToArray();
            content = Encoding.UTF8.GetString(byteContent);
        }

        public string ReadContent() => content;

        public void WriteContent(string content) => this.content = content;

        public void Persist(TarOutputStream target)
        {
            string path = this.TarPath();
            TarEntry entry = TarEntry.CreateTarEntry(path);
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            entry.Size = byteContent.Length;
            target.PutNextEntry(entry);
            target.Write(byteContent, 0, byteContent.Length);
            target.CloseEntry();
        }
    }
}
