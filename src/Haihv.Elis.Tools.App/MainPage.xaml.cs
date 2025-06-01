using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.Maui.Extensions;

namespace Haihv.Elis.Tools.App;

public partial class MainPage
{
    private ConnectionInfo _connectionInfo = null!;

    private readonly ConnectionService _connectionService;

    public MainPage(ConnectionService connectionService)
    {
        InitializeComponent();
        _connectionService = connectionService;
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
                var (success, message) = await _connectionInfo.CheckConnection();
                if (success)
                {
                    UpdateConnectionInfoUi(); // Cập nhật UI với thông tin kết nối đã đọc
                    await DisplayAlert("Thông báo", "Đọc và kiểm tra thông tin kết nối đã lưu thành công!", "OK");
                    _connectionService.ConnectionInfo = _connectionInfo;
                    ExportDataBtn.IsEnabled = true; // Kích hoạt nút Export nếu kết nối thành công
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
                ExportDataBtn.IsEnabled = true;
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

    private async void CheckConnectionBtn_Clicked(object? sender, EventArgs e)
    {
        try
        {
            ExportDataBtn.IsEnabled = false; // Vô hiệu hóa nút Export khi đang kiểm tra kết nối
            // Cập nhật dữ liệu từ UI vào ConnectionInfo (phòng trường hợp binding không hoạt động)
            _connectionInfo.Server = EntryServer.Text;
            _connectionInfo.Database = EntryDatabase.Text;
            _connectionInfo.Username = EntryUserId.Text;
            _connectionInfo.Password = EntryPassword.Text;

            var (success, message) = await _connectionInfo.CheckConnection();
            if (success)
            {
                await DisplayAlert("Thông báo", "Kết nối thành công!", "OK");
                await SaveConnection();
            }
            else
            {
                await DisplayAlert("Lỗi", $"Kết nối thất bại: {message}", "OK");
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Lỗi", $"Không thể kiểm tra kết nối: {exception.Message}", "OK");
        }
    }

    private async void ImportDataBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Vô hiệu hóa nút Export khi đang import
            ExportDataBtn.IsEnabled = false;
            // Sử dụng cách tiếp cận đơn giản hơn cho file picker
            var pickOptions = new PickOptions
            {
                PickerTitle = "Chọn file thông tin kết nối"
            };

            var result = await FilePicker.Default.PickAsync(pickOptions);
            if (result != null)
                // Đọc thông tin kết nối từ file (tự động thử cả mã hóa và không mã hóa)
                await ProcessConnectionImport(result.FullPath);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            // Xử lý các lỗi cụ thể
            if (ex.Message.Contains("This platform does not support this file type"))
                errorMessage = "File type không được hỗ trợ. Vui lòng chọn file .inf, .json hoặc .txt";
            else if (ex.Message.Contains("No file was selected"))
                // Người dùng hủy chọn file, không cần hiển thị thông báo lỗi
                return;

            await DisplayAlert("Lỗi", $"Không thể import thông tin kết nối: {errorMessage}", "OK");
        }
    }

    private async Task ProcessConnectionImport(string filePath)
    {
        try
        {
            var (importedConnection, message) = await filePath.ImportConnectionSettings();
            if (importedConnection is not null)
            {
                _connectionInfo = importedConnection;
                // Cập nhật UI với thông tin kết nối đã import
                UpdateConnectionInfoUi();
                await SaveConnection();
            }

            await DisplayAlert("Thông báo",
                string.IsNullOrEmpty(message) ? "Tệp thông tin kết nối chính xác!" : $"Import thành công!\n{message}",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể đọc thông tin kết nối từ file: {ex.Message}", "OK");
        }
    }

    private async void ExportDataBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Lưu thông tin kết nối vào file (có mã hóa)
            var (success, message) = await _connectionInfo.ExportConnectionSettings();

            if (!success)
            {
                await DisplayAlert("Lỗi", message, "OK");
                return;
            }

            // Thông báo lưu thành công và hỏi có muốn chia sẻ không
            var shareChoice = await DisplayAlert("Thông báo",
                $"Xuất tệp thành công, tại vị trí:\n{message}\n\nBạn có muốn chia sẻ file này không?",
                "Chia sẻ", "Mở vị trí file");

            if (shareChoice)
                // Người dùng chọn chia sẻ
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Chia sẻ file thông tin kết nối",
                    File = new ShareFile(message)
                });
            else
                // Người dùng chọn mở vị trí file
                await FileHelper.OpenFileLocation(message);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể export thông tin kết nối: {ex.Message}", "OK");
        }
    }
}