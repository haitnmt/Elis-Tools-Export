namespace Haihv.Elis.Tools.Export.App.Shared.Settings.Cache;

public static class DataConnection
{
    private const string RootKey = "Data:Connection:";
    public static string KeyConnectionString  => $"{RootKey}:ConnectionString";
    public const string KeyConnectionInfo = $"{RootKey}:ConnectionInfo";
}