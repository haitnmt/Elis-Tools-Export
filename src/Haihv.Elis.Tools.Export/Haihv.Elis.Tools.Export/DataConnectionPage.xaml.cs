using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Haihv.Elis.Tools.Export;

public partial class DataConnectionPage : ContentPage
{
    public DataConnectionPage()
    {
        InitializeComponent();
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && !string.IsNullOrEmpty(entry.Text))
        {
            // Sử dụng Dispatcher để đảm bảo việc select text được thực hiện sau khi focus hoàn tất
            Dispatcher.Dispatch(() =>
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text.Length;
            });
        }
    }

    private async void OnConnectButtonClicked(object? sender, EventArgs e)
    {
        // Hiển thị trạng thái đang kết nối
        StatusLabel.Text = "Đang kiểm tra kết nối...";
        StatusLabel.TextColor = Colors.Blue;
        ConnectButton.IsEnabled = false;

        try
        {
            // Xây dựng connection string
            var connectionString = BuildConnectionString();

            // Kiểm tra kết nối
            await TestConnectionAsync(connectionString);

            // Kết nối thành công
            StatusLabel.Text = "✅ Kết nối thành công!";
            StatusLabel.TextColor = Colors.Green;

            await DisplayAlert("Thành công", "Kết nối đến SQL Server thành công!", "OK");
        }
        catch (Exception ex)
        {
            // Kết nối thất bại
            StatusLabel.Text = "❌ Kết nối thất bại!";
            StatusLabel.TextColor = Colors.Red;

            await DisplayAlert("Lỗi kết nối", $"Không thể kết nối đến SQL Server:\n{ex.Message}", "OK");
        }
        finally
        {
            ConnectButton.IsEnabled = true;
        }
    }

    private string BuildConnectionString()
    {
        var server = ServerEntry.Text?.Trim();
        var database = DatabaseEntry.Text?.Trim();
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(server))
            throw new ArgumentException("Vui lòng nhập tên máy chủ SQL Server");

        if (string.IsNullOrEmpty(database))
            throw new ArgumentException("Vui lòng nhập tên cơ sở dữ liệu");

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            ConnectTimeout = 10,
            TrustServerCertificate = true
        };

        // Nếu có username và password thì dùng SQL Authentication
        if (!string.IsNullOrEmpty(username))
        {
            builder.UserID = username;
            builder.Password = password ?? "";
            builder.IntegratedSecurity = false;
        }
        else
        {
            // Dùng Windows Authentication
            builder.IntegratedSecurity = true;
        }

        return builder.ConnectionString;
    }

    private async Task TestConnectionAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Thực hiện một query đơn giản để kiểm tra kết nối
        using var command = new SqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync();
    }
}