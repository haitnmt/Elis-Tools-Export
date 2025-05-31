using Haihv.Elis.Tools.Data.Models;

namespace Haihv.Elis.Tools.App.Settings
{
    /// <summary>
    /// Helper class để đọc/ghi file trực tiếp mà không cần dependency injection
    /// </summary>
    internal static class FileHelper
    {
        /// <summary>
        /// Đọc file văn bản
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <param name="cancellationToken">Token hủy bỏ</param>
        /// <returns>Nội dung file hoặc null nếu file không tồn tại</returns>
        internal static async Task<string?> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
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
        internal static async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
        {
            try
            {
                // Tạo thư mục nếu chưa tồn tại
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, content, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi ghi file {filePath}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Kiểm tra file có tồn tại không
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <returns>True nếu file tồn tại</returns>
        internal static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Đọc thông tin kết nối từ file
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <param name="encrypted">File có được mã hóa không</param>
        /// <param name="cancellationToken">Token hủy bỏ</param>
        /// <returns>Thông tin kết nối hoặc null nếu không đọc được</returns>
        internal static async Task<ConnectionInfo?> LoadConnectionInfoAsync(string filePath, bool encrypted = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var content = await ReadFileAsync(filePath, cancellationToken);
                if (string.IsNullOrEmpty(content))
                    return null;

                return ConnectionInfo.DeserializeConnectionInfo(content, encrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc thông tin kết nối từ file {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu thông tin kết nối vào file
        /// </summary>
        /// <param name="filePath">Đường dẫn file</param>
        /// <param name="connectionInfo">Thông tin kết nối</param>
        /// <param name="encrypted">Có mã hóa file không</param>
        /// <param name="cancellationToken">Token hủy bỏ</param>
        /// <returns>True nếu lưu thành công</returns>
        internal static async Task<bool> SaveConnectionInfoAsync(string filePath, ConnectionInfo? connectionInfo, bool encrypted = true, CancellationToken cancellationToken = default)
        {
            try
            {
                if (connectionInfo == null || !connectionInfo.IsValid())
                    return false;

                var serializedData = ConnectionInfo.Serialize(connectionInfo, encrypted);
                await WriteFileAsync(filePath, serializedData, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu thông tin kết nối vào file {filePath}: {ex.Message}");
                return false;
            }
        }
    }
}
