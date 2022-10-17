using System.Text;

namespace RandoSettingsManager.SettingsManagement.Filer.Tar
{
    internal static class FilerExtensions
    {
        public static string TarPath(this IDirectory dir)
        {
            StringBuilder path = new(dir.Name.TrimPathSeparator());
            IDirectory? current = dir.Parent;
            while (current != null)
            {
                path.Insert(0, $"{current.Name}/");
                current = current.Parent;
            }
            return path.Append('/').ToString().TrimStart('/');
        }

        private static string TrimPathSeparator(this string p) => p.Replace('\\', '/')
            .Trim('/');

        public static string TarPath(this IFile file)
        {
            return file.Parent.TarPath() + file.Name;
        }
    }
}
