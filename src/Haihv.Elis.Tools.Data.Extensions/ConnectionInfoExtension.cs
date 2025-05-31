using System.Text.Json;
using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Services;
using Haihv.Extensions.String;
using Microsoft.Data.SqlClient;

namespace Haihv.Elis.Tools.Data.Extensions;

public static class ConnectionInfoExtension
{
    public static async Task<ConnectionInfo?> LoadConnectionInfoAsync(this IFileService fileService, string filePath,
        bool encrypted = true,
        CancellationToken cancellationToken = default)
    {
        if (!await fileService.ExistsAsync(filePath))
        {
            return null;
        }

        var connectionStringEncrypt = await fileService.ReadFileAsync(filePath, cancellationToken);
        return ConnectionInfo.DeserializeConnectionInfo(connectionStringEncrypt, encrypted);
    }

    public static async Task<bool> SaveConnectionInfoAsync(this IFileService fileService, string filePath,
        ConnectionInfo? connectionInfo, bool encrypted = true,
        CancellationToken cancellationToken = default)
    {
        if (connectionInfo == null || !connectionInfo.IsValid())
        {
            return false;
        }

        var json = JsonSerializer.Serialize(connectionInfo);
        if (encrypted)
        {
            json = json.Encrypt();
        }

        await fileService.WriteFileAsync(filePath, json, cancellationToken);
        return true;
    }

    public static async Task<(bool sucsess, string message)> CheckConnection(this ConnectionInfo connectionInfo)
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

    public static async Task<(ConnectionInfo? connectionInfo, string message)> ImportConnectionSettings(
        IFileService fileService)
    {
        try
        {
            var fileContent = await fileService.PickFileAsync(
                allowedFileTypes: [".inf", ".json", ".txt"],
                title: "Chọn file cấu hình kết nối");

            if (string.IsNullOrEmpty(fileContent))
            {
                return (null, "Không có file nào được chọn.");
            }

            var importedConnectionInfo = ConnectionInfo.DeserializeConnectionInfo(fileContent, encrypted: true) ??
                                         // Thử giải mã không có encryption
                                         ConnectionInfo.DeserializeConnectionInfo(fileContent, encrypted: false);

            if (importedConnectionInfo == null || !importedConnectionInfo.IsValid())
                return (null, "File cấu hình không hợp lệ hoặc bị hỏng.");

            return (importedConnectionInfo, string.Empty);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    public static async Task<(bool sucsess, string message)> ExportConnectionSettings(this IFileService fileService,
        ConnectionInfo connectionInfo)
    {
        try
        {
            if (!connectionInfo.IsValid())
            {
                return (false, "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.");
            }

            var exportContent = ConnectionInfo.Serialize(connectionInfo, encrypted: true);
            var fileName = $"ConnectionInfo_{connectionInfo.Server.Replace('.', '_')}_{connectionInfo.Database}.inf";

            var success = await fileService.SaveFileAsAsync(
                content: exportContent,
                suggestedFileName: fileName,
                title: "Lưu file cấu hình kết nối");

            return (success, success ? string.Empty : "Việc export cấu hình kết nối đã bị hủy hoặc không thành công.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}