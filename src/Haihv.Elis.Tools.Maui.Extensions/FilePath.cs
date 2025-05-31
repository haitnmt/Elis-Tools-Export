namespace Haihv.Elis.Tools.Maui.Extensions
{
    internal static class FilePath
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

        internal static string CacheOnDisk => Path.Combine(PathRootConfig(), "CacheFiles");

        internal static string LogFile(string fileName) => Path.Combine(PathRootConfig("Logs", true), fileName);
        internal static string PathConnectionString =>
            Path.Combine(PathRootConfig(), "ConnectionInfo.inf");
    }
}
