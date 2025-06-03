using Haihv.Extensions.String;
using System.Text.Json;

namespace Haihv.Elis.Tools.Data.Models;

public sealed class ConnectionInfo
{
    public string Server { get; set; } = "localhost";

    public string Database { get; set; } = "elis";

    public string Username { get; set; } = "sa";

    public string Password { get; set; } = "123456";
    public bool UseIntegratedSecurity { get; set; }
    public int ConnectTimeout { get; set; } = 10;

    public int DvhcRootId { get; set; } = 27;

    /// <summary>
    /// Thời gian hết hạn của tệp kết nối. Mặc định là null (không hết hạn)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Kiểm tra xem tệp kết nối có còn hiệu lực hay không
    /// </summary>
    /// <returns>True nếu tệp chưa hết hạn hoặc không có thời gian hết hạn</returns>
    public bool IsNotExpired()
    {
        return ExpiryDate == null || DateTime.Now <= ExpiryDate.Value;
    }

    /// <summary>
    /// Đặt thời gian hết hạn từ ngày hiện tại + số ngày
    /// </summary>
    /// <param name="days">Số ngày từ hiện tại</param>
    public void SetExpiryFromNow(int days)
    {
        ExpiryDate = DateTime.Now.AddDays(days);
    }

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

    /// <summary>
    /// Deserialize chuỗi JSON thành đối tượng ConnectionInfo.
    /// </summary>
    /// <param name="json">Chuỗi JSON chứa thông tin kết nối.</param>
    /// <param name="secretKey">Khóa bí mật để giải mã thông tin kết nối (nếu cần).</param>
    /// <param name="encrypted">Chỉ định xem thông tin kết nối có được mã hóa hay không.</param>
    /// <returns></returns>
    public static ConnectionInfo? DeserializeConnectionInfo(string json, string? secretKey = null, bool encrypted = true)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            // Nếu có khóa bí mật, sử dụng mã hóa mạnh với mật khẩu
            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                var decryptedJson = EncryptionHelper.DecryptWithPassword(json, secretKey);
                return JsonSerializer.Deserialize<ConnectionInfo>(decryptedJson);
            }
            // Nếu không có khóa bí mật, sử dụng mã hóa mặc định
            if (encrypted)
            {
                json = json.Decrypt();
            }
            // Giải mã chuỗi JSON thành đối tượng ConnectionInfo
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

    /// <summary>
    /// Serialize thông tin kết nối thành chuỗi JSON.
    /// </summary>
    /// <param name="connectionInfo">Thông tin kết nối cần serialize.</param>
    /// <param name="secretKey">Khóa bí mật để mã hóa thông tin kết nối (nếu cần).</param>
    /// <param name="encrypted">Chỉ định xem thông tin kết nối có được mã hóa hay không.</param>
    /// <returns></returns>
    public static string Serialize(ConnectionInfo connectionInfo, string? secretKey = null, bool encrypted = true)
    {
        ArgumentNullException.ThrowIfNull(connectionInfo);
        try
        {
            var json = JsonSerializer.Serialize(connectionInfo, SWriteOptions);

            // Mã hóa thông tin kết nối nếu cần
            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                return EncryptionHelper.EncryptWithPassword(json, secretKey);
            }

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
