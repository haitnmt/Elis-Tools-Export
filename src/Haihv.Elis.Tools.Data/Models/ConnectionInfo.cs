using Haihv.Extensions.String;
using System.Text.Json;

namespace Haihv.Elis.Tools.Data.Models;

public sealed class ConnectionInfo
{
    public string Server { get; set; } = "localhost";

    public string Database { get; set; } = "elis";

    public string Username { get; set; } = "sa";

    public string Password { get;set; } = "123456";

    public bool UseIntegratedSecurity { get; set; }
    public int ConnectTimeout { get; set; } = 10;
    
    public string ToConnectionString()
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = Database,
            ConnectTimeout = ConnectTimeout,
            TrustServerCertificate = true
        };

        if (UseIntegratedSecurity || string.IsNullOrEmpty(Username))
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = Username;
            builder.Password = Password;
            builder.IntegratedSecurity = false;
        }

        return builder.ConnectionString;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Server) && !string.IsNullOrEmpty(Database);
    }
    public static ConnectionInfo? DeserializeConnectionInfo(string json, bool encrypted = true)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            // Nếu thông tin kết nối đã được mã hóa, giải mã nó
            if (encrypted)
            {
                json = json.Decrypt();
            }
            return JsonSerializer.Deserialize<ConnectionInfo>(json);
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Lỗi khi giải mã thông tin kết nối: {ex.Message}");
            return null;
        }
    }

    private static readonly JsonSerializerOptions SWriteOptions = new()
    {
        WriteIndented = true
    };
    public static string Serialize(ConnectionInfo connectionInfo, bool encrypted = true)
    {
        ArgumentNullException.ThrowIfNull(connectionInfo);
        try
        {
            var json = JsonSerializer.Serialize(connectionInfo, SWriteOptions);

            // Mã hóa thông tin kết nối nếu cần
            return encrypted ? json.Encrypt() : json;
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Lỗi khi serialize thông tin kết nối: {ex.Message}");
            throw;
        }

    }

    public string RenderConnectionInfo()

    => IsValid() ?
        $"Server: {Server} | Database: {Database} | User: {Username}" :
        "Thông tin kết nối không hợp lệ. Vui lòng kiểm tra lại.";

}
