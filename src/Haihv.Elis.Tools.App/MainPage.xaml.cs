using Haihv.Elis.Tools.App.Extensions;
using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.Maui.Extensions;
using System.Text;

namespace Haihv.Elis.Tools.App;

public partial class MainPage
{
    private ConnectionInfo _connectionInfo = null!;
    private ConnectionInfo? _lastValidConnectionInfo = null; // Lưu trữ thông tin kết nối đã kiểm tra thành công gần nhất

    private readonly ConnectionService _connectionService; public MainPage(ConnectionService connectionService)
    {
        InitializeComponent();
        _connectionService = connectionService;

        // Đăng ký sự kiện TextChanged cho các Entry
        EntryServer.TextChanged += OnConnectionInfoChanged;
        EntryDatabase.TextChanged += OnConnectionInfoChanged;
        EntryUserId.TextChanged += OnConnectionInfoChanged;
        EntryPassword.TextChanged += OnConnectionInfoChanged;

        // Đọc thông tin kết nối từ file cấu hình
        _ = LoadConnectionInfoAsync();
    }

    private async Task LoadConnectionInfoAsync()
    {
        try
        {
            // Đọc thông tin kết nối từ file cấu hình sử dụng FileHelper
            var loadedInfo = await ConnectionInfoExtension.LoadConnectionInfoAsync();

            if (loadedInfo != null && loadedInfo.IsValid())
            {
                _connectionInfo = loadedInfo;
                var (success, message) = await _connectionInfo.CheckConnection(); if (success)
                {
                    UpdateConnectionInfoUi(); // Cập nhật UI với thông tin kết nối đã đọc
                    await DisplayAlert("Thông báo", "Đọc và kiểm tra thông tin kết nối đã lưu thành công!", "OK"); _connectionService.ConnectionInfo = _connectionInfo;
                    _lastValidConnectionInfo = CloneConnectionInfo(_connectionInfo); // Lưu thông tin kết nối đã kiểm tra thành công
                    ShareConnectionBtn.IsEnabled = true; // Kích hoạt nút Share nếu kết nối thành công
                }
                else
                {
                    await DisplayAlert("Lỗi",
                        $"Kết nối thất bại: {message}\nVui lòng sửa và kiểm tra lại thông tin kết nối!", "OK");
                }

                return;
            }
        }
        catch (Exception ex)
        {
            // Ghi log lỗi nếu cần
            System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc file cấu hình: {ex.Message}");
        }

        // Nếu không đọc được từ file hoặc dữ liệu không hợp lệ, sử dụng giá trị mặc định
        SetDefaultConnectionInfo();
    }
    private void UpdateConnectionInfoUi()
    {
        // Cập nhật các trường nhập liệu từ ConnectionInfo
        EntryServer.Text = _connectionInfo.Server;
        EntryDatabase.Text = _connectionInfo.Database;
        EntryUserId.Text = _connectionInfo.Username;
        EntryPassword.Text = _connectionInfo.Password;

        // Cập nhật trạng thái nút ShareConnectionBtn
        UpdateShareButtonState();
    }

    private void SetDefaultConnectionInfo()
    {
        _connectionInfo = new ConnectionInfo
        {
            Server = "localhost",
            Database = "ElisTools",
            Username = "sa",
            Password = "123456"
        };
        // Cập nhật UI với thông tin kết nối mặc định
        UpdateConnectionInfoUi();
        _connectionService.ConnectionInfo = _connectionInfo;
    }
    private async Task SaveConnection()
    {
        try
        {
            // Lưu thông tin kết nối vào file sau khi kết nối thành công
            var (success, message) = await _connectionInfo.SaveConnectionInfoAsync();
            if (success)
            {
                _connectionService.ConnectionInfo = _connectionInfo; // Cập nhật thông tin kết nối
                _lastValidConnectionInfo = CloneConnectionInfo(_connectionInfo); // Lưu thông tin kết nối đã kiểm tra thành công
                ShareConnectionBtn.IsEnabled = true;
            }
            else
            {
                await DisplayAlert("Lỗi", $"Không thể lưu thông tin kết nối:\n {message}", "OK");
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Lỗi", $"Không thể kiểm tra kết nối: {exception.Message}", "OK");
        }
    }

    /// <summary>
    /// Sự kiện được gọi khi thông tin kết nối thay đổi
    /// </summary>
    private void OnConnectionInfoChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateShareButtonState();
    }

    /// <summary>
    /// Cập nhật trạng thái của nút ShareConnectionBtn dựa trên sự thay đổi thông tin kết nối
    /// </summary>
    private void UpdateShareButtonState()
    {
        if (_lastValidConnectionInfo == null)
        {
            ShareConnectionBtn.IsEnabled = false;
            return;
        }

        // Lấy thông tin hiện tại từ UI
        var currentInfo = new ConnectionInfo
        {
            Server = EntryServer.Text ?? string.Empty,
            Database = EntryDatabase.Text ?? string.Empty,
            Username = EntryUserId.Text ?? string.Empty,
            Password = EntryPassword.Text ?? string.Empty
        };

        // So sánh với thông tin đã kiểm tra thành công gần nhất
        bool isUnchanged = IsConnectionInfoEqual(currentInfo, _lastValidConnectionInfo);
        ShareConnectionBtn.IsEnabled = isUnchanged;
    }

    /// <summary>
    /// So sánh hai ConnectionInfo để xem có giống nhau không
    /// </summary>
    private static bool IsConnectionInfoEqual(ConnectionInfo info1, ConnectionInfo info2)
    {
        if (info1 == null || info2 == null) return false;

        return string.Equals(info1.Server, info2.Server, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(info1.Database, info2.Database, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(info1.Username, info2.Username, StringComparison.Ordinal) &&
               string.Equals(info1.Password, info2.Password, StringComparison.Ordinal);
    }

    /// <summary>
    /// Tạo bản sao của ConnectionInfo
    /// </summary>
    private static ConnectionInfo CloneConnectionInfo(ConnectionInfo original)
    {
        return new ConnectionInfo
        {
            Server = original.Server,
            Database = original.Database,
            Username = original.Username,
            Password = original.Password,
            UseIntegratedSecurity = original.UseIntegratedSecurity,
            ConnectTimeout = original.ConnectTimeout
        };
    }

    private async void CheckConnectionBtn_Clicked(object? sender, EventArgs e)
    {
        try
        {
            ShareConnectionBtn.IsEnabled = false; // Vô hiệu hóa nút Export khi đang kiểm tra kết nối
            // Cập nhật dữ liệu từ UI vào ConnectionInfo (phòng trường hợp binding không hoạt động)
            _connectionInfo.Server = EntryServer.Text;
            _connectionInfo.Database = EntryDatabase.Text;
            _connectionInfo.Username = EntryUserId.Text;
            _connectionInfo.Password = EntryPassword.Text;

            var (success, message) = await _connectionInfo.CheckConnection(); if (success)
            {
                await DisplayAlert("Thông báo", "Kết nối thành công!", "OK");
                await SaveConnection();
            }
            else
            {
                ShareConnectionBtn.IsEnabled = false; // Disable nút nếu kết nối thất bại
                await DisplayAlert("Lỗi", $"Kết nối thất bại: {message}", "OK");
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Lỗi", $"Không thể kiểm tra kết nối: {exception.Message}", "OK");
        }
    }

    private async void OpenConnectionFileBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Vô hiệu hóa nút Export khi đang mở tệp
            ShareConnectionBtn.IsEnabled = false;

            // Sử dụng cách tiếp cận đơn giản hơn cho file picker
            var pickOptions = new PickOptions
            {
                PickerTitle = "Chọn tệp kết nối để mở"
            };

            var result = await FilePicker.Default.PickAsync(pickOptions);
            if (result != null)
            {
                // Thử mở mà không cần mật khẩu trước
                await ProcessConnectionOpen(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            // Xử lý các lỗi cụ thể
            if (ex.Message.Contains("This platform does not support this file type"))
                errorMessage = "Định dạng tệp không được hỗ trợ. Vui lòng chọn tệp .inf, .json hoặc .txt";
            else if (ex.Message.Contains("No file was selected"))
                // Người dùng hủy chọn file, không cần hiển thị thông báo lỗi
                return;

            await DisplayAlert("Lỗi mở tệp", $"Không thể mở thông tin kết nối:\n{errorMessage}", "OK");
        }
    }

    private async Task ProcessConnectionOpen(string filePath)
    {
        try
        {
            // Thử mở mà không cần mật khẩu trước (tệp không mã hóa)
            var (openedConnection, message) = await filePath.ImportConnectionSettings(string.Empty);
            if (openedConnection is not null)
            {                // Mở thành công mà không cần mật khẩu
                _connectionInfo = openedConnection;
                UpdateConnectionInfoUi();
                await SaveConnection();
                await DisplayAlert("✅ Mở tệp thành công",
                    "Đã mở thông tin kết nối từ tệp không mã hóa thành công!", "OK");
                return;
            }
        }
        catch (Exception)
        {            // Nếu thất bại, có thể tệp được mã hóa, yêu cầu nhập mật khẩu
        }

        // Nếu không mở được mà không có mật khẩu, tệp có thể được mã hóa
        await RequestPasswordAndOpen(filePath);
    }

    private async Task RequestPasswordAndOpen(string filePath)
    {
        while (true)
        {
            // Yêu cầu nhập mật khẩu mã hóa với ký tự ẩn (****)
            var secretKey = await this.DisplayPasswordPromptAsync(
                "🔒 Nhập mật khẩu giải mã",
                "Tệp được mã hóa. Vui lòng nhập mật khẩu để giải mã tệp kết nối:");

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                await DisplayAlert("Hủy mở tệp", "Bạn đã hủy việc mở tệp kết nối!", "OK"); return;
            }

            // Thử mở với mật khẩu đã nhập
            var success = await TryOpenWithPassword(filePath, secretKey);
            if (success)
                break;

            // Nếu mật khẩu sai, tiếp tục vòng lặp để nhập lại
        }
    }
    private async Task<bool> TryOpenWithPassword(string filePath, string secretKey)
    {
        try
        {
            // Sử dụng mật khẩu mã hóa khi mở thông tin kết nối
            var (openedConnection, message) = await filePath.ImportConnectionSettings(secretKey); if (openedConnection is not null)
            {
                _connectionInfo = openedConnection;
                // Cập nhật UI với thông tin kết nối đã mở
                UpdateConnectionInfoUi();
                await SaveConnection();

                await DisplayAlert("✅ Mở tệp thành công",
                    "Đã mở và giải mã thông tin kết nối thành công!", "OK");
                return true;
            }
            else
            {
                await DisplayAlert("❌ Mật khẩu không chính xác",
                    "Không thể giải mã tệp với mật khẩu đã nhập.\nVui lòng kiểm tra lại mật khẩu!", "Thử lại");
                return false;
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("password") || ex.Message.Contains("decrypt") || ex.Message.Contains("mã hóa"))
            {
                await DisplayAlert("❌ Mật khẩu không chính xác",
                    "Mật khẩu giải mã không đúng. Vui lòng thử lại!", "Thử lại");
                return false;
            }
            else
            {
                await DisplayAlert("❌ Lỗi mở tệp", $"Không thể đọc thông tin kết nối từ tệp:\n{ex.Message}", "OK");
                return false;
            }
        }
    }

    private async void ShareConnectionFileBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            var sharePage = new Controls.ShareConnectionPage(_connectionInfo);
            await Navigation.PushModalAsync(sharePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể mở trang chia sẻ: {ex.Message}", "OK");
        }
    }
}

