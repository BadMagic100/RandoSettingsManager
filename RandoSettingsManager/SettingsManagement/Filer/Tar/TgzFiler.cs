using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.IO;

namespace RandoSettingsManager.SettingsManagement.Filer.Tar
{
    internal class TgzFiler
    {
        private TarDirectory dir;
        public IDirectory RootDirectory => dir;

        private TgzFiler(TarDirectory dir)
        {
            this.dir = dir;
        }

        public void WriteAll(Stream str)
        {
            using GZipOutputStream g = new(str);
            g.SetLevel(9);
            using TarOutputStream tar = new(g);
            dir.Persist(tar);
        }

        public static TgzFiler LoadFromStream(Stream str)
        {
            using GZipInputStream g = new(str);
            using TarInputStream tar = new(g);
            return new(TarDirectory.Load(tar));
        }

        public static TgzFiler CreateForWrite() => new(new TarDirectory());
    }
}
