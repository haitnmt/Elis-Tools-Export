namespace Haihv.Elis.Tools.Export.App.Shared.Settings
{
    public static class FilePath
    {
        private static string PathRootConfig(string folder = "", bool addDate = false)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ElisExport");
            if (!string.IsNullOrWhiteSpace(folder))
            {
                path = Path.Combine(path, folder);
            }

            if (addDate)
            {
                path = Path.Combine(path, DateTime.Now.ToString("yyyy-MM-dd"));
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
        public static string CacheOnDisk => Path.Combine(PathRootConfig(), "CacheFiles");

        public static string LogFile(string fileName) => Path.Combine(PathRootConfig("Logs", true), fileName);
        public static string PathConnectionString =>
            Path.Combine(PathRootConfig(), "ConnectionInfo.inf");
    }
}
