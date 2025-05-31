using Haihv.Extensions.String;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Haihv.Elis.Tools.Data.Models;

public class ConnectionInfo : INotifyPropertyChanged
{
    private string _server = "localhost";
    private string _database = "elis";
    private string _username = "sa";
    private string _password = "123456";
    private bool _useIntegratedSecurity;
    private int _connectTimeout = 10;

    public string Server
    {
        get => _server;
        set => SetProperty(ref _server, value);
    }

    public string Database
    {
        get => _database;
        set => SetProperty(ref _database, value);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool UseIntegratedSecurity
    {
        get => _useIntegratedSecurity;
        set => SetProperty(ref _useIntegratedSecurity, value);
    }

    public int ConnectTimeout
    {
        get => _connectTimeout;
        set => SetProperty(ref _connectTimeout, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
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
        string.Empty;

}
