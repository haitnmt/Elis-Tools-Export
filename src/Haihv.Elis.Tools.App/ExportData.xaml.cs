using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;
using Microsoft.Maui.Controls;

namespace Haihv.Elis.Tools.App;

public partial class ExportData : ContentPage
{

	private readonly ConnectionInfo? _connectionInfo;
	public ExportData(ConnectionService connectionService)
	{
		_connectionInfo = connectionService.ConnectionInfo;

		// Khởi tạo giao diện
		InitializeComponent();
		DataInfoLabel.Text = _connectionInfo?.RenderConnectionInfo() ?? "Chưa có thông tin kết nối";
	}
}