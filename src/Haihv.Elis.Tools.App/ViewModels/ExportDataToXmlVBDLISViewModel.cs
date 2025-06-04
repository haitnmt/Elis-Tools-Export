using Haihv.Elis.Tools.Data.Services;
using System.ComponentModel;

namespace Haihv.Elis.Tools.App.ViewModels;

class ExportDataToXmlVBDLISViewModel : INotifyPropertyChanged
{

    private readonly string _connectionString = string.Empty;
    private readonly int _rootDvhcId = 0;
    private readonly Page _parentPage;

    private readonly bool _databaseReady;

    #region Commands


    #endregion

    public ExportDataToXmlVBDLISViewModel(ConnectionService connectionService, Page parentPage)
    {
        _connectionString = connectionService.ConnectionInfo?.ToConnectionString() ?? string.Empty;
        _rootDvhcId = connectionService.ConnectionInfo?.DvhcRootId ?? 0;
        _parentPage = parentPage;
        _databaseReady = connectionService.HasLoadedInitialConnection;

    }
    #region Properties
    public bool DatabaseReady => _databaseReady;
    #endregion
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
