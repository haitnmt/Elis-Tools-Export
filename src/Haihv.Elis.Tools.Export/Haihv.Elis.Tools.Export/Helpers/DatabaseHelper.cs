using Microsoft.Data.SqlClient;
using Haihv.Elis.Tools.Export.Models;
using Haihv.Elis.Tools.Export.Services;

namespace Haihv.Elis.Tools.Export.Helpers;

public class DatabaseHelper : IDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        if (_connection == null)
        {
            _connection = new SqlConnection(_connectionString);
            await _connection.OpenAsync();
        }
        return _connection;
    }

    public async Task<object?> ExecuteScalarAsync(string sql)
    {
        var connection = await GetConnectionAsync();
        using var command = new SqlCommand(sql, connection);
        return await command.ExecuteScalarAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _connection = null;
    }
}

public static class DatabaseHelperExtensions
{
    public static ConnectionInfo? GetCurrentConnection()
    {
        try
        {
            var connectionService = ServiceHelper.GetService<IConnectionService>();
            return connectionService.CurrentConnection;
        }
        catch
        {
            return null;
        }
    }

    public static string? GetCurrentConnectionString()
    {
        var connection = GetCurrentConnection();
        return connection?.ToConnectionString();
    }

    public static bool HasValidConnection()
    {
        var connection = GetCurrentConnection();
        return connection?.IsValid() == true;
    }

    public static async Task<bool> LoadConnectionAsync()
    {
        try
        {
            var connectionService = ServiceHelper.GetService<IConnectionService>();
            var connection = await connectionService.LoadConnectionAsync();
            return connection?.IsValid() == true;
        }
        catch
        {
            return false;
        }
    }
}
