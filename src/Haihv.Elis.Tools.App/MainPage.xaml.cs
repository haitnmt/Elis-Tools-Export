using Haihv.Elis.Tools.App.Settings;
using Haihv.Elis.Tools.Data.Extensions;
using Haihv.Elis.Tools.Data.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#if ANDROID
using AndroidX.Core.Content;
#endif

namespace Haihv.Elis.Tools.App;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private ConnectionInfo _connectionInfo = null!;

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

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChangedCustom(propertyName);
        return true;
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
            var loadedInfo = await FileHelper.LoadConnectionInfoAsync(FilePath.PathConnectionString, encrypted: true);

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
        _ = SaveConnectionInfoAsync();
    }
    private async Task OnCheckConnection()
    {
        // Cập nhật dữ liệu từ UI vào ConnectionInfo (phòng trường hợp binding không hoạt động)
        ConnectionInfo.Server = EntryServer.Text ?? "";
        ConnectionInfo.Database = EntryDatabase.Text ?? "";
        ConnectionInfo.Username = EntryUserId.Text ?? "";
        ConnectionInfo.Password = EntryPassword.Text ?? "";

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

    private void CheckConnectionBtn_Clicked(object sender, EventArgs e)
    {
        _ = OnCheckConnection();
    }
    private async Task SaveConnectionInfoAsync()
    {
        try
        {
            // Lưu thông tin kết nối vào file sử dụng FileHelper
            var success = await FileHelper.SaveConnectionInfoAsync(FilePath.PathConnectionString, ConnectionInfo, encrypted: true);

            if (success)
            {
                await DisplayAlert("Thông báo", "Lưu thông tin kết nối thành công!", "OK");
            }
            else
            {
                await DisplayAlert("Thông báo", "Không có thông tin kết nối để lưu!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể lưu thông tin kết nối: {ex.Message}", "OK");
        }
    }
    private async void ImportDataBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.data", "public.json" } },
                    { DevicePlatform.Android, new[] { "application/octet-stream", "application/json" } },
                    { DevicePlatform.WinUI, new[] { ".inf", ".json" } },
                    { DevicePlatform.macOS, new[] { "inf", "json" } },
                });

            var pickOptions = new PickOptions
            {
                PickerTitle = "Chọn file thông tin kết nối (.inf hoặc .json)",
                FileTypes = customFileType,
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
            await DisplayAlert("Lỗi", $"Không thể import thông tin kết nối: {ex.Message}", "OK");
        }
    }

    private async Task ProcessConnectionImport(string filePath)
    {
        try
        {
            ConnectionInfo? importedConnection = null;

            // Đầu tiên thử đọc file mã hóa (.inf)
            if (Path.GetExtension(filePath).Equals(".inf", StringComparison.OrdinalIgnoreCase))
            {
                importedConnection = await FileHelper.LoadConnectionInfoAsync(filePath, encrypted: true);
            }
            // Nếu không thành công hoặc là file .json, thử đọc không mã hóa
            else if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                importedConnection = await FileHelper.LoadConnectionInfoAsync(filePath, encrypted: false);
            }

            if (importedConnection != null && importedConnection.IsValid())
            {
                // Cập nhật thông tin kết nối
                ConnectionInfo = importedConnection;
                OnPropertyChangedCustom(nameof(ConnectionInfo));

                // Cập nhật UI
                await Dispatcher.DispatchAsync(() =>
                {
                    EntryServer.Text = ConnectionInfo.Server;
                    EntryDatabase.Text = ConnectionInfo.Database;
                    EntryUserId.Text = ConnectionInfo.Username;
                    EntryPassword.Text = ConnectionInfo.Password;
                });

                await DisplayAlert("Thành công", "Import thông tin kết nối thành công!", "OK");
            }
            else
            {
                await DisplayAlert("Lỗi", "File không chứa thông tin kết nối hợp lệ hoặc file bị hỏng!", "OK");
            }
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
            ConnectionInfo.Server = EntryServer.Text ?? "";
            ConnectionInfo.Database = EntryDatabase.Text ?? "";
            ConnectionInfo.Username = EntryUserId.Text ?? "";
            ConnectionInfo.Password = EntryPassword.Text ?? "";

            // Kiểm tra có thông tin kết nối để export không
            if (ConnectionInfo == null || !ConnectionInfo.IsValid())
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin kết nối trước khi export!", "OK");
                return;
            }

            // Tạo file trong thư mục Download
            var fileName = $"elis_connection_{ConnectionInfo.Database}_{DateTime.Now:yyyyMMdd_HHmmss}.inf";
            var downloadPath = GetDownloadFolderPath();
            var filePath = Path.Combine(downloadPath, fileName);

            // Lưu thông tin kết nối vào file (có mã hóa)
            var success = await FileHelper.SaveConnectionInfoAsync(filePath, ConnectionInfo, encrypted: true);

            if (!success)
            {
                await DisplayAlert("Lỗi", "Không thể tạo file export!", "OK");
                return;
            }

            // Thông báo lưu thành công và hỏi có muốn chia sẻ không
            var shareChoice = await DisplayAlert("Thành công",
                $"Export thành công!\nFile đã được lưu tại: {filePath}\n\nBạn có muốn chia sẻ file này không?",
                "Chia sẻ", "Mở vị trí file");

            if (shareChoice)
            {
                // Người dùng chọn chia sẻ
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Chia sẻ file thông tin kết nối",
                    File = new ShareFile(filePath)
                });
            }
            else
            {
                // Người dùng chọn mở vị trí file
                await OpenFileLocation(filePath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể export thông tin kết nối: {ex.Message}", "OK");
        }
    }

    private string GetDownloadFolderPath()
    {
        try
        {
#if WINDOWS
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif MACCATALYST
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif ANDROID
            return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath 
                   ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Downloads");
#elif IOS
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
#else
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif
        }
        catch
        {
            // Fallback về thư mục temp nếu không thể truy cập Downloads
            return Path.GetTempPath();
        }
    }

    private async Task OpenFileLocation(string filePath)
    {
        try
        {
#if WINDOWS
            // Mở Windows Explorer và highlight file
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
#elif MACCATALYST
            // Mở Finder và highlight file
            System.Diagnostics.Process.Start("open", $"-R \"{filePath}\"");
#else
            // Cho các platform khác, chỉ hiển thị thông báo với đường dẫn
            await DisplayAlert("Vị trí file", $"File đã được lưu tại:\n{filePath}", "OK");
#endif
        }
        catch (Exception ex)
        {
            // Fallback: hiển thị đường dẫn file
            await DisplayAlert("Vị trí file", $"File đã được lưu tại:\n{filePath}\n\nLỗi mở thư mục: {ex.Message}", "OK");
        }
    }
}