using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Haihv.Elis.Tools.Maui.Extensions
{
    /// <summary>
    /// Helper class để đọc/ghi file trực tiếp mà không cần dependency injection
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Đọc file văn bản
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <param name="cancellationToken">Token hủy bỏ</param>
        /// <returns>Nội dung file hoặc null nếu file không tồn tại</returns>
        public static async Task<string?> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                return await File.ReadAllTextAsync(filePath, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ghi nội dung vào file
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <param name="content">Nội dung cần ghi</param>
        /// <param name="cancellationToken">Token hủy bỏ</param>
        public static async Task WriteFileAsync(string filePath, string content,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Kiểm tra đường dẫn
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("Đường dẫn file không được rỗng", nameof(filePath));
                }

                // Tạo thư mục nếu chưa tồn tại
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Ghi file với timeout để tránh hang
                await File.WriteAllTextAsync(filePath, content, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Lỗi không xác định: {filePath}");
                System.Diagnostics.Debug.WriteLine($"   Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"   Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <returns>True nếu file tồn tại</returns>
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        public static string GetDownloadFolderPath()
        {
            try
            {
                #if WINDOWS
                            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                #elif MACCATALYST
                                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                #elif ANDROID
                            return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath 
                                   ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Downloads");
                #elif IOS
                            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
                #else
                            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                #endif
            }
            catch
            {
                // Fallback về thư mục temp nếu không thể truy cập Downloads
                return Path.GetTempPath();
            }
        }

        public static Task<string> OpenFileLocation(string filePath)
        {
            try
            {
                #if WINDOWS
                            // Mở Windows Explorer và highlight file
                            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                            return Task.FromResult(string.Empty);
                #elif MACCATALYST
                            // Mở Finder và highlight file
                            System.Diagnostics.Process.Start("open", $"-R \"{filePath}\"");
                            return Task.FromResult(string.Empty);
                #else
                            // Cho các platform khác, chỉ hiển thị thông báo với đường dẫn
                            return $"File đã được lưu tại:\n{filePath}";
                #endif
            }
            catch (Exception ex)
            {
                // Fallback: hiển thị đường dẫn file
                return Task.FromResult($"File đã được lưu tại:\n{filePath}\n\nLỗi mở thư mục: {ex.Message}");
            }
        }
        internal static string PathRootConfig(string folder = "", bool addDate = false)
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
        public static string LogFile(string fileName) => Path.Combine(PathRootConfig("Logs", true), fileName);
    }
}
