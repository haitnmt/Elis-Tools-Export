using Haihv.Elis.Tools.Export.Models;
using Haihv.Elis.Tools.Export.Services;
using Microsoft.Data.SqlClient;

namespace Haihv.Elis.Tools.Export;

public partial class DataConnectionDialog
{
    public event EventHandler? CloseRequested;
    public event EventHandler<ConnectionInfo>? ConnectionSaved;

    private readonly IConnectionService _connectionService;

    public DataConnectionDialog()
    {
        InitializeComponent();
        _connectionService = ServiceHelper.GetService<IConnectionService>();
        _ = LoadSavedConnectionAsync();
    }

    private async Task LoadSavedConnectionAsync()
    {
        try
        {
            var savedConnection = await _connectionService.LoadConnectionAsync();
            if (savedConnection != null)
            {
                Dispatcher.Dispatch(() =>
                {
                    ServerEntry.Text = savedConnection.Server;
                    DatabaseEntry.Text = savedConnection.Database;
                    UsernameEntry.Text = savedConnection.Username;
                    PasswordEntry.Text = savedConnection.Password;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi tải thông tin kết nối: {ex.Message}");
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && !string.IsNullOrEmpty(entry.Text))
        {
            Dispatcher.Dispatch(() =>
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text.Length;
            });
        }
    }

    private void OnOverlayTapped(object sender, TappedEventArgs e)
    {
        // Click vào overlay (nền tối) để đóng dialog
        CloseDialog();
    }
    private void OnDialogTapped(object sender, TappedEventArgs e)
    {
        // Ngăn việc đóng dialog khi click vào nội dung dialog
        // Không cần xử lý gì thêm
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        CloseDialog();
    }

    private void CloseDialog()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        TestButton.IsEnabled = false;
        TestButton.Text = "Đang kiểm tra...";

        try
        {
            var connectionInfo = BuildConnectionInfo();
            var connectionString = connectionInfo.ToConnectionString();
            await TestConnectionAsync(connectionString);

            // Lưu thông tin kết nối khi test thành công
            await _connectionService.SaveConnectionAsync(connectionInfo);
            ConnectionSaved?.Invoke(this, connectionInfo);

            await DisplayAlert("Thành công", "Kết nối đến SQL Server thành công!\nThông tin kết nối đã được lưu.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi kết nối", $"Không thể kết nối đến SQL Server:\n{ex.Message}", "OK");
        }
        finally
        {
            TestButton.IsEnabled = true;
            TestButton.Text = "Kiểm tra kết nối";
        }
    }
    private ConnectionInfo BuildConnectionInfo()
    {
        var server = ServerEntry.Text?.Trim() ?? string.Empty;
        var database = DatabaseEntry.Text?.Trim() ?? string.Empty;
        var username = UsernameEntry.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrEmpty(server))
            throw new ArgumentException("Vui lòng nhập tên máy chủ SQL Server");

        if (string.IsNullOrEmpty(database))
            throw new ArgumentException("Vui lòng nhập tên cơ sở dữ liệu");

        return new ConnectionInfo
        {
            Server = server,
            Database = database,
            Username = username,
            Password = password,
            UseIntegratedSecurity = string.IsNullOrEmpty(username),
            ConnectTimeout = 10
        };
    }

    private string BuildConnectionString()
    {
        var connectionInfo = BuildConnectionInfo();
        return connectionInfo.ToConnectionString();
    }

    private async Task TestConnectionAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync();
    }

    private async Task DisplayAlert(string title, string message, string cancel)
    {
        if (Parent is Page page)
        {
            await page.DisplayAlert(title, message, cancel);
        }
        else
        {
            // Fallback để tìm page gần nhất
            var currentElement = Parent;
            while (currentElement != null && currentElement is not Page)
            {
                currentElement = currentElement.Parent;
            }

            if (currentElement is Page foundPage)
            {
                await foundPage.DisplayAlert(title, message, cancel);
            }
        }
    }
}
