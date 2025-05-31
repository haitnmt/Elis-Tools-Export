using Haihv.Elis.Tools.App.Settings;
using Haihv.Elis.Tools.Data.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Haihv.Elis.Tools.Maui.Extensions;

namespace Haihv.Elis.Tools.App;

public partial class MainPage : INotifyPropertyChanged
{
    private ConnectionInfo _connectionInfo = null!;
    private readonly string _filePath = FilePath.PathConnectionString;
    public ConnectionInfo ConnectionInfo
    {
        get => _connectionInfo;
        set => SetProperty(ref _connectionInfo, value);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChangedCustom([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

        backingStore = value;
        OnPropertyChangedCustom(propertyName);
    }

    public MainPage()
    {
        InitializeComponent();

        // Đọc thông tin kết nối từ file cấu hình
        _ = LoadConnectionInfoAsync();

        // Thiết lập BindingContext cho toàn bộ trang
        BindingContext = this;
    }
    private async Task LoadConnectionInfoAsync()
    {
        try
        {
            // Đọc thông tin kết nối từ file cấu hình sử dụng FileHelper
            var loadedInfo = await _filePath.LoadConnectionInfoAsync(encrypted: true);

            if (loadedInfo != null && loadedInfo.IsValid())
            {
                ConnectionInfo = loadedInfo;
                OnPropertyChangedCustom(nameof(ConnectionInfo));
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

    private void SetDefaultConnectionInfo()
    {
        ConnectionInfo = new ConnectionInfo
        {
            Server = "localhost",
            Database = "ElisTools",
            Username = "sa",
            Password = "123456",
        };
        OnPropertyChangedCustom(nameof(ConnectionInfo));
    }

    private void SaveConnectionBtn_Clicked(object? sender, EventArgs e)
    {
        // Lưu thông tin kết nối vào file sau khi kết nối thành công
        var (success, message) =  _connectionInfo.SaveConnectionInfoAsync(_filePath).Result;
        if (success)
        {
            DisplayAlert("Thành công", "Thông tin kết nối đã được lưu thành công!", "OK");
        }
        else
        {
            DisplayAlert("Lỗi", $"Không thể lưu thông tin kết nối: {message}", "OK");
        }
    }

    private async void CheckConnectionBtn_Clicked(object? sender, EventArgs e)
    {
        try
        {
            // Cập nhật dữ liệu từ UI vào ConnectionInfo (phòng trường hợp binding không hoạt động)
            ConnectionInfo.Server = EntryServer.Text;
            ConnectionInfo.Database = EntryDatabase.Text;
            ConnectionInfo.Username = EntryUserId.Text;
            ConnectionInfo.Password = EntryPassword.Text;

            var (success, message) = await ConnectionInfo.CheckConnection();
            if (success)
            {
                await DisplayAlert("Thành công", "Kết nối thành công!", "OK");
                SaveConnectionBtn.IsEnabled = true;
            }
            else
            {
                await DisplayAlert("Lỗi", $"Kết nối thất bại: {message}", "OK");
                SaveConnectionBtn.IsEnabled = false;
            }
        }
        catch (Exception exception)
        {
            await DisplayAlert("Lỗi", $"Không thể kiểm tra kết nối: {exception.Message}", "OK");
            SaveConnectionBtn.IsEnabled = false;
        }
    }
    
    private async void ImportDataBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Sử dụng cách tiếp cận đơn giản hơn cho file picker
            var pickOptions = new PickOptions
            {
                PickerTitle = "Chọn file thông tin kết nối",
            };

            var result = await FilePicker.Default.PickAsync(pickOptions);
            if (result != null)
            {
                // Đọc thông tin kết nối từ file (tự động thử cả mã hóa và không mã hóa)
                await ProcessConnectionImport(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            
            // Xử lý các lỗi cụ thể
            if (ex.Message.Contains("This platform does not support this file type"))
            {
                errorMessage = "File type không được hỗ trợ. Vui lòng chọn file .inf, .json hoặc .txt";
            }
            else if (ex.Message.Contains("No file was selected"))
            {
                // Người dùng hủy chọn file, không cần hiển thị thông báo lỗi
                return;
            }
            
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
                ConnectionInfo = importedConnection;
            }
            await DisplayAlert("Thông báo", 
                string.IsNullOrEmpty(message) ? 
                    "Import thành công!" : 
                    $"Import thành công!\n{message}", "OK");
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
            // Cập nhật thông tin kết nối từ UI trước khi export
            ConnectionInfo.Server = EntryServer.Text;
            ConnectionInfo.Database = EntryDatabase.Text;
            ConnectionInfo.Username = EntryUserId.Text;
            ConnectionInfo.Password = EntryPassword.Text;
            
            // Lưu thông tin kết nối vào file (có mã hóa)
            var (success, message) = await ConnectionInfo.ExportConnectionSettings();

            if (!success)
            {
                await DisplayAlert("Lỗi", message, "OK");
                return;
            }

            // Thông báo lưu thành công và hỏi có muốn chia sẻ không
            var shareChoice = await DisplayAlert("Thành công",
                $"Xuất tệp thành công, tại vị trí:\n{message}\n\nBạn có muốn chia sẻ file này không?",
                "Chia sẻ", "Mở vị trí file");

            if (shareChoice)
            {
                // Người dùng chọn chia sẻ
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Chia sẻ file thông tin kết nối",
                    File = new ShareFile(message)
                });
            }
            else
            {
                // Người dùng chọn mở vị trí file
                await FileHelper.OpenFileLocation(message);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể export thông tin kết nối: {ex.Message}", "OK");
        }
    }
}