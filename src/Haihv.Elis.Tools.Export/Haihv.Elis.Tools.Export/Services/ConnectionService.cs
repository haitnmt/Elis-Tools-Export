using Haihv.Elis.Tools.Export.Extensions;
using Haihv.Elis.Tools.Export.Models;
using System.Text.Json;

namespace Haihv.Elis.Tools.Export.Services;

public interface IConnectionService
{
    Task<ConnectionInfo?> LoadConnectionAsync();
    Task SaveConnectionAsync(ConnectionInfo connectionInfo);
    Task ClearConnectionAsync();
    ConnectionInfo? CurrentConnection { get; }
    event EventHandler<ConnectionInfo?>? ConnectionChanged;
}

public class ConnectionService : IConnectionService
{
    private const string ConnectionFileName = "connection.json";
    private readonly string _connectionFilePath;
    private ConnectionInfo? _currentConnection;

    public ConnectionInfo? CurrentConnection => _currentConnection;
    public event EventHandler<ConnectionInfo?>? ConnectionChanged;

    public ConnectionService()
    {
        var appDataPath = FileSystem.AppDataDirectory;
        _connectionFilePath = Path.Combine(appDataPath, ConnectionFileName);
    }

    public async Task<ConnectionInfo?> LoadConnectionAsync()
    {
        try
        {
            if (!File.Exists(_connectionFilePath))
                return null;

            var json = await File.ReadAllTextAsync(_connectionFilePath);
            if (string.IsNullOrEmpty(json))
                return null;

            // Giải mã thông tin kết nối trước khi deserializing
            json = json.Decrypt();
            var connectionInfo = JsonSerializer.Deserialize<ConnectionInfo>(json);
            _currentConnection = connectionInfo;
            ConnectionChanged?.Invoke(this, _currentConnection);
            return connectionInfo;
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông tin kết nối: {ex.Message}");
            return null;
        }
    }

    private static readonly JsonSerializerOptions s_writeOptions = new()
    {
        WriteIndented = true
    };

    public async Task SaveConnectionAsync(ConnectionInfo connectionInfo)
    {
        try
        {
            var json = JsonSerializer.Serialize(connectionInfo, s_writeOptions);

            // Mã hóa thông tin kết nối trước khi lưu
            json = json.Encrypt();
            await File.WriteAllTextAsync(_connectionFilePath, json);
            _currentConnection = connectionInfo;
            ConnectionChanged?.Invoke(this, _currentConnection);
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu thông tin kết nối: {ex.Message}");
            throw;
        }
    }
    public Task ClearConnectionAsync()
    {
        try
        {
            if (File.Exists(_connectionFilePath))
            {
                File.Delete(_connectionFilePath);
            }
            _currentConnection = null;
            ConnectionChanged?.Invoke(this, _currentConnection);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa thông tin kết nối: {ex.Message}");
            throw;
        }
    }
}
