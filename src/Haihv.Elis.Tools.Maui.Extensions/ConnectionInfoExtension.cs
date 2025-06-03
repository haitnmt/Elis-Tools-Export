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
    /// <param name="secretKey">Khóa bí mật để mã hóa thông tin kết nối (nếu cần)</param>
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>Thông tin kết nối hoặc null nếu không đọc được</returns>
    private static async Task<ConnectionInfo?> LoadConnectionInfoAsync(this string filePath, string? secretKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var content = await FileHelper.ReadFileAsync(filePath, cancellationToken);
            if (string.IsNullOrEmpty(content))
            {
                System.Diagnostics.Debug.WriteLine($"File {filePath} không tồn tại hoặc rỗng.");
                return null;
            }
            return string.IsNullOrWhiteSpace(secretKey)
                ? ConnectionInfo.DeserializeConnectionInfo(content)
                : ConnectionInfo.DeserializeConnectionInfo(content, secretKey);
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
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>Thông tin kết nối hoặc null nếu không đọc được</returns>
    public static async Task<ConnectionInfo?> LoadConnectionInfoAsync(CancellationToken cancellationToken = default)
        => await LoadConnectionInfoAsync(PathConnectionString, cancellationToken: cancellationToken);

    /// <summary>
    /// Lưu thông tin kết nối vào file
    /// </summary>
    /// <param name="connectionInfo">Thông tin kết nối</param>
    /// <param name="filePath">Đường dẫn file để lưu thông tin kết nối</param>
    /// <param name="secretKey">Khóa bí mật để mã hóa thông tin kết nối (nếu cần)</param>
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>True nếu lưu thành công</returns>
    private static async Task<(bool success, string messager)> SaveConnectionInfoAsync(this ConnectionInfo? connectionInfo,
        string filePath, string? secretKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, "Đường dẫn file lưu thông tin kết nối không hợp lệ.");
            if (connectionInfo == null || !connectionInfo.IsValid())
                return (false, "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.");
            var serializedData = string.IsNullOrWhiteSpace(secretKey)
                ? ConnectionInfo.Serialize(connectionInfo)
                : ConnectionInfo.Serialize(connectionInfo, secretKey);

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
    /// <param name="cancellationToken">Token hủy bỏ</param>
    /// <returns>True nếu lưu thành công</returns>
    public static async Task<(bool success, string messager)> SaveConnectionInfoAsync(this ConnectionInfo? connectionInfo,
        CancellationToken cancellationToken = default)
    => await SaveConnectionInfoAsync(connectionInfo, PathConnectionString, cancellationToken: cancellationToken);

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
            const string sql = """
                                SELECT   COUNT(GCNQSDD.MaGCN) FROM 
                                GCNQSDD INNER JOIN DangKyQSDD ON GCNQSDD.MaDangKy = DangKyQSDD.MaDangKy INNER JOIN 
                                ThuaDat ON DangKyQSDD.MaThuaDat = ThuaDat.MaThuaDat INNER JOIN
                                ToBanDo ON ThuaDat.MaToBanDo = ToBanDo.MaToBanDo
                                WHERE   (LEN(GCNQSDD.SoSerial) > 8) AND 
                                (ToBanDo.MaDVHC IN (SELECT [MaDVHC] FROM [DVHC] WHERE MaDVHC > 10000 AND (MaTinh = @MaDVHC OR MaHuyen = @MaDVHC OR MaXa = @MaDVHC)))
                                """;
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@MaDVHC", connectionInfo.DvhcRootId);
            var count = await command.ExecuteScalarAsync() as int? ?? -1;
            return count <= 0
                ? (false, "Không có thông tin Giấy chứng nhận hợp lệ nào trong cơ sở dữ liệu.")
                : (true, "Kết nối đến cơ sở dữ liệu thành công.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public static async Task<(ConnectionInfo? connectionInfo, string message)> ImportConnectionSettings(this string filePath,
        string? secretKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var importedConnection = await LoadConnectionInfoAsync(filePath, secretKey, cancellationToken);

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
        string? secretKey = null, CancellationToken cancellationToken = default)
    {
        var file = string.Empty;
        try
        {
            // Kiểm tra có thông tin kết nối để export không
            if (connectionInfo == null || !connectionInfo.IsValid())
            {
                return (false, "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.");
            }

            // Tạo đường dẫn file để lưu thông tin kết nối
            var fileName = $"elis_connection_{connectionInfo.Database}_{DateTime.Now:yyyyMMdd_HHmmss}.inf";
            var downloadPath = FileHelper.GetDownloadFolderPath();
            file = Path.Combine(downloadPath, fileName);

            // Lưu thông tin kết nối vào file (có mã hóa)
            var (success, message) = await connectionInfo.SaveConnectionInfoAsync(file, secretKey, cancellationToken);
            return (success, success ? file : $"Lỗi khi xuất thông tin kết nối: {message}");

        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(file))
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xuất thông tin kết nối vào file {file}: {ex.Message}");
            else
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xuất thông tin kết nối: {ex.Message}");
            return (false, $"Lỗi khi xuất thông tin kết nối: {ex.Message}");
        }
    }
}