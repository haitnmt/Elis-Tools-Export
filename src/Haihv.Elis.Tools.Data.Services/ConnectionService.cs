using Haihv.Elis.Tools.Data.Models;

namespace Haihv.Elis.Tools.Data.Services;

public class ConnectionService
{
    public ConnectionInfo? ConnectionInfo { get; set; }
    public bool HasLoadedInitialConnection { get; set; } = false;

}