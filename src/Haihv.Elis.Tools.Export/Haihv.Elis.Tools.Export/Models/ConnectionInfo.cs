namespace Haihv.Elis.Tools.Export.Models;

public class ConnectionInfo
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseIntegratedSecurity { get; set; } = true;
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
}
