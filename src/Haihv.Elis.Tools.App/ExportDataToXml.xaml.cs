using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;

namespace Haihv.Elis.Tools.App;

public partial class ExportDataToXml : ContentPage
{
    private readonly ConnectionInfo? _connectionInfo;

    public ExportDataToXml(ConnectionService connectionService)
    {
        _connectionInfo = connectionService.ConnectionInfo;

        // Khởi tạo giao diện
        InitializeComponent();
        DataInfoLabel.Text = _connectionInfo?.RenderConnectionInfo() ?? "Chưa có thông tin kết nối";
    }
}