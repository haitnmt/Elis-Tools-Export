using Haihv.Elis.Tools.Export.Services;
using Haihv.Elis.Tools.Export.Models;

namespace Haihv.Elis.Tools.Export
{
    public partial class MainPage
    {
        private int _count;
        private readonly IConnectionService _connectionService;

        public MainPage(IConnectionService connectionService)
        {
            InitializeComponent();
            _connectionService = connectionService;

            // Đăng ký event để theo dõi thay đổi kết nối
            _connectionService.ConnectionChanged += OnConnectionChanged;

            // Tải thông tin kết nối khi khởi tạo
            _ = LoadConnectionInfoAsync();
        }

        private async Task LoadConnectionInfoAsync()
        {
            try
            {
                await _connectionService.LoadConnectionAsync();
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông tin kết nối: {ex.Message}");
            }
        }

        private void OnConnectionChanged(object? sender, ConnectionInfo? connectionInfo)
        {
            Dispatcher.Dispatch(() => UpdateConnectionStatus());
        }

        private void UpdateConnectionStatus()
        {
            var connection = _connectionService.CurrentConnection;
            if (connection != null && connection.IsValid())
            {
                ConnectionStatusLabel.Text = "✅ Đã kết nối cơ sở dữ liệu";
                ConnectionStatusLabel.TextColor = Colors.Green;
                ConnectionDetailsLabel.Text = $"Server: {connection.Server}\nDatabase: {connection.Database}";
                TestQueryBtn.IsEnabled = true;
                System.Diagnostics.Debug.WriteLine($"Đã có kết nối: {connection.Server}/{connection.Database}");
            }
            else
            {
                ConnectionStatusLabel.Text = "❌ Chưa có kết nối cơ sở dữ liệu";
                ConnectionStatusLabel.TextColor = Colors.Red;
                ConnectionDetailsLabel.Text = "";
                TestQueryBtn.IsEnabled = false;
            }
        }

        private void OnDataConnectionClicked(object? sender, EventArgs e)
        {
            // Hiển thị dialog
            ConnectionDialog.IsVisible = true;
        }
        private void OnDialogCloseRequested(object? sender, EventArgs e)
        {
            // Ẩn dialog
            ConnectionDialog.IsVisible = false;
        }

        private void OnConnectionSaved(object? sender, ConnectionInfo connectionInfo)
        {
            // Được gọi khi thông tin kết nối được lưu thành công
            UpdateConnectionStatus();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            _count++;

            if (_count == 1)
                CounterBtn.Text = $"Clicked {_count} time";
            else
                CounterBtn.Text = $"Clicked {_count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnTestQueryClicked(object? sender, EventArgs e)
        {
            var connection = _connectionService.CurrentConnection;
            if (connection == null || !connection.IsValid())
            {
                await DisplayAlert("Lỗi", "Không có kết nối cơ sở dữ liệu", "OK");
                return;
            }

            TestQueryBtn.IsEnabled = false;
            TestQueryBtn.Text = "Đang thực hiện...";

            try
            {
                using var dbHelper = new Helpers.DatabaseHelper(connection.ToConnectionString());
                var result = await dbHelper.ExecuteScalarAsync("SELECT GETDATE()");

                await DisplayAlert("Thành công", $"Kết nối thành công!\nThời gian server: {result}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Lỗi khi thực hiện truy vấn:\n{ex.Message}", "OK");
            }
            finally
            {
                TestQueryBtn.IsEnabled = true;
                TestQueryBtn.Text = "Test truy vấn dữ liệu";
            }
        }
    }
}
