using Haihv.Elis.Tools.Export.Services;
using Haihv.Elis.Tools.Export.Helpers;
using Microsoft.Data.SqlClient;

namespace Haihv.Elis.Tools.Export.Demo;

/// <summary>
/// Class demo cách sử dụng connection trong toàn bộ ứng dụng
/// </summary>
public class DataExportDemo
{
    private readonly IConnectionService _connectionService;

    public DataExportDemo(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    /// <summary>
    /// Export danh sách tables từ database hiện tại
    /// </summary>
    public async Task<List<string>> GetTableListAsync()
    {
        var connection = _connectionService.CurrentConnection;
        if (connection == null || !connection.IsValid())
        {
            throw new InvalidOperationException("Không có kết nối cơ sở dữ liệu");
        }

        var tables = new List<string>();

        using var dbHelper = new DatabaseHelper(connection.ToConnectionString());
        using var sqlConnection = await dbHelper.GetConnectionAsync();
        using var command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", sqlConnection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    /// <summary>
    /// Lấy số lượng records của một table
    /// </summary>
    public async Task<int> GetRecordCountAsync(string tableName)
    {
        var connection = _connectionService.CurrentConnection;
        if (connection == null || !connection.IsValid())
        {
            throw new InvalidOperationException("Không có kết nối cơ sở dữ liệu");
        }

        using var dbHelper = new DatabaseHelper(connection.ToConnectionString());
        var result = await dbHelper.ExecuteScalarAsync($"SELECT COUNT(*) FROM [{tableName}]");
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// Kiểm tra connection có sẵn
    /// </summary>
    public bool IsConnectionAvailable()
    {
        return DatabaseHelperExtensions.HasValidConnection();
    }

    /// <summary>
    /// Lấy thông tin connection hiện tại
    /// </summary>
    public string GetConnectionInfo()
    {
        var connection = _connectionService.CurrentConnection;
        if (connection == null)
            return "Không có kết nối";

        return $"Server: {connection.Server}, Database: {connection.Database}";
    }
}
