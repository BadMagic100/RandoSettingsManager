namespace RandoSettingsManager.SettingsManagement.Filer
{
    internal interface IFile
    {
        public string Name { get; }

        public IDirectory Parent { get; }

        public void WriteContent(string content);
        public string ReadContent();

        public void Delete();
    }
}
