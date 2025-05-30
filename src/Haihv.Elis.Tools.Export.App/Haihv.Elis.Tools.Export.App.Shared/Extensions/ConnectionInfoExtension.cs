using System.Text.Json;
using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Export.App.Shared.Services;
using Haihv.Extensions.String;

namespace Haihv.Elis.Tools.Export.App.Shared.Extensions;

public static class ConnectionInfoExtension
{
    public static async Task<ConnectionInfo?> LoadConnectionInfoAsync(this IFileService fileService, string filePath, bool encrypted = true, 
        CancellationToken cancellationToken = default)
    {
        if (!await fileService.ExistsAsync(filePath))
        {
            return null;
        }

        var connectionStringEncrypt = await fileService.ReadFileAsync(filePath, cancellationToken);
        return ConnectionInfo.DeserializeConnectionInfo(connectionStringEncrypt, encrypted);
    }
    
    public static async Task<bool> SaveConnectionInfoAsync(this IFileService fileService, string filePath, ConnectionInfo? connectionInfo, bool encrypted = true,
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
}
