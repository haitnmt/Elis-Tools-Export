using Haihv.Elis.Tools.Data.Models;
using Microsoft.Data.SqlClient;

namespace Haihv.Elis.Tools.Maui.Extensions;

public static class ConnectionInfoExtension
{
    private static string PathConnectionString =>
        Path.Combine(FileHelper.PathRootConfig(), "ConnectionInfo.inf");
    /// <summary>
    /// Đọc thông tin kết nối từ file
    /// </summary>
    /// <param name="filePath">Đường dẫn file</param>
    /// <param name="encrypted">File có được mã hóa không</param>
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>Thông tin kết nối hoặc null nếu không đọc được</returns>
    private static async Task<ConnectionInfo?> LoadConnectionInfoAsync(this string filePath, bool encrypted = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var content = await FileHelper.ReadFileAsync(filePath, cancellationToken);
            return string.IsNullOrEmpty(content) ? null : ConnectionInfo.DeserializeConnectionInfo(content, encrypted);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc thông tin kết nối từ file {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Đọc thông tin kết nối từ file
    /// </summary>
    /// <param name="encrypted">File có được mã hóa không</param>
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>Thông tin kết nối hoặc null nếu không đọc được</returns>
    public static async Task<ConnectionInfo?> LoadConnectionInfoAsync(bool encrypted = true,
        CancellationToken cancellationToken = default)
        => await LoadConnectionInfoAsync(PathConnectionString, encrypted, cancellationToken);

    /// <summary>
    /// Lưu thông tin kết nối vào file
    /// </summary>
    /// <param name="connectionInfo">Thông tin kết nối</param>
    /// <param name="filePath">Đường dẫn file để lưu thông tin kết nối</param>
    /// <param name="encrypted">Có mã hóa file không</param>
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>True nếu lưu thành công</returns>
    private static async Task<(bool success, string messager)> SaveConnectionInfoAsync(this ConnectionInfo? connectionInfo, 
        string filePath, bool encrypted = true, CancellationToken cancellationToken = default)
    {
        try
        {
            if (connectionInfo == null || !connectionInfo.IsValid())
                return (false, "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.");
            var serializedData = ConnectionInfo.Serialize(connectionInfo, encrypted);
            await FileHelper.WriteFileAsync(filePath, serializedData, cancellationToken);
            return (true, "Lưu thông tin kết nối thành công.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu thông tin kết nối vào file {filePath}: {ex.Message}");
            return (false, $"Lỗi khi lưu thông tin kết nối: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Lưu thông tin kết nối vào file
    /// </summary>
    /// <param name="connectionInfo">Thông tin kết nối</param>
    /// <param name="encrypted">Có mã hóa file không</param>
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>True nếu lưu thành công</returns>
    public static async Task<(bool success, string messager)> SaveConnectionInfoAsync(this ConnectionInfo? connectionInfo, 
        bool encrypted = true, CancellationToken cancellationToken = default)
    => await SaveConnectionInfoAsync(connectionInfo, PathConnectionString, encrypted, cancellationToken);

    public static async Task<(bool success, string message)> CheckConnection(this ConnectionInfo connectionInfo)
    {
        try
        {
            if (!connectionInfo.IsValid())
            {
                return (false, "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.");
            }

            var connectionString = connectionInfo.ToConnectionString();
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            const string sql = "SELECT count(GCNQSDD.MaGCN) FROM GCNQSDD WHERE LEN(SoSerial) > 8";
            await using var command = new SqlCommand(sql, connection);
            var count = await command.ExecuteScalarAsync() as int? ?? -1;
            return count < 0
                ? (false, "Không có thông tin Giấy chứng nhận hợp lệ nào trong cơ sở dữ liệu.")
                : (true, "Kết nối đến cơ sở dữ liệu thành công.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(ConnectionInfo? connectionInfo, string message)> ImportConnectionSettings(this string filePath)
    {
        try
        {
            ConnectionInfo? importedConnection = null;
            var fileExtension = Path.GetExtension(filePath);

            // Đầu tiên thử đọc file mã hóa (.inf hoặc file không có extension)
            if (fileExtension.Equals(".inf", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(fileExtension))
            {
                importedConnection = await LoadConnectionInfoAsync(filePath, encrypted: true);
            }
            
            // Nếu không thành công và là file .json hoặc .txt, thử đọc không mã hóa
            if (importedConnection == null && (
                fileExtension.Equals(".json", StringComparison.OrdinalIgnoreCase) ||
                fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase)))
            {
                importedConnection = await LoadConnectionInfoAsync(filePath, encrypted: false);
            }
            
            // Nếu vẫn không thành công, thử cả hai phương thức (fallback)
            importedConnection ??= await LoadConnectionInfoAsync(filePath) ??
                                   await LoadConnectionInfoAsync(filePath, encrypted: false);

            if (importedConnection != null && importedConnection.IsValid())
            {
                return (importedConnection, string.Empty);
            }

            return (null, "Không thể đọc thông tin kết nối từ file. Vui lòng kiểm tra định dạng file.");
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(bool success, string message)> ExportConnectionSettings(this ConnectionInfo? connectionInfo, 
        string? filePath = null, CancellationToken cancellationToken = default)
    {
        try
        {

            // Kiểm tra có thông tin kết nối để export không
            if (connectionInfo == null || !connectionInfo.IsValid())
            {
                return (false, "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.");
            }

            string file;
            // Tạo file trong thư mục Download
            if (string.IsNullOrEmpty(filePath))
            {
                var fileName = $"elis_connection_{connectionInfo.Database}_{DateTime.Now:yyyyMMdd_HHmmss}.inf";
                var downloadPath = FileHelper.GetDownloadFolderPath();
                file = Path.Combine(downloadPath, fileName);
            }
            else
            {
                file = filePath;
            }
            
            // Lưu thông tin kết nối vào file (có mã hóa)
            var (success, message) = await connectionInfo.SaveConnectionInfoAsync(file, encrypted: true, cancellationToken);
            return (success, success? file : $"Lỗi khi xuất thông tin kết nối: {message}");

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi xuất thông tin kết nối vào file {filePath}: {ex.Message}");
            return (false, $"Lỗi khi xuất thông tin kết nối: {ex.Message}");
        }
    }
}