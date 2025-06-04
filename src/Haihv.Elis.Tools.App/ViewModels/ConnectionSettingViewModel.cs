using Haihv.Elis.Tools.App.ContentPages;
using Haihv.Elis.Tools.App.Extensions;
using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.Maui.Extensions;
using System.ComponentModel;
using System.Windows.Input;

namespace Haihv.Elis.Tools.App.ViewModels;

public sealed class ConnectionSettingViewModel : INotifyPropertyChanged
{
    private ConnectionInfo _connectionInfo = null!;
    private ConnectionInfo? _lastValidConnectionInfo;
    private readonly ConnectionService _connectionService;
    private readonly Page _parentPage;

    private string _server = "localhost";
    private string _database = "elis";
    private string _userId = "sa";
    private string _password = "1234567";
    private int _dvhcRootId = 27; // Mặc định là 27 (Bắc Ninh)
    private bool _isShareButtonEnabled;
    public ConnectionSettingViewModel(ConnectionService connectionService, Page parentPage)
    {
        _connectionService = connectionService;
        _parentPage = parentPage;


        CheckConnectionCommand = new Command(async void () => await CheckConnectionAsync());
        OpenConnectionFileCommand = new Command(async void () => await OpenConnectionFileAsync());
        ShareConnectionFileCommand = new Command(async void () => await ShareConnectionFileAsync());
        LoadConnectionCommand = new Command(async void () => await LoadConnectionInfoAsync());

        // Chỉ đọc thông tin kết nối từ file cấu hình lần đầu tiên (khi khởi động ứng dụng)
        if (!_connectionService.HasLoadedInitialConnection)
        {
            _ = LoadConnectionInfoAsync();
        }
        else if (_connectionService.ConnectionInfo != null)
        {
            // Nếu đã có thông tin kết nối trong service, sử dụng nó
            _connectionInfo = _connectionService.ConnectionInfo;
            UpdateConnectionInfoProperties();
            _lastValidConnectionInfo = CloneConnectionInfo(_connectionInfo);
            IsShareButtonEnabled = true;
        }
    }

    #region Properties

    public string Server
    {
        get => _server;
        set
        {
            _server = value;
            OnPropertyChanged();
            OnConnectionInfoChanged();
        }
    }

    public string Database
    {
        get => _database;
        set
        {
            _database = value;
            OnPropertyChanged();
            OnConnectionInfoChanged();
        }
    }

    public string UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            OnPropertyChanged();
            OnConnectionInfoChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
            OnConnectionInfoChanged();
        }
    }
    public bool IsShareButtonEnabled
    {
        get => _isShareButtonEnabled;
        set
        {
            _isShareButtonEnabled = value;
            OnPropertyChanged();
        }
    }
    public int DvhcRootId
    {
        get => _dvhcRootId;
        set
        {
            _dvhcRootId = value;
            OnPropertyChanged();
            OnConnectionInfoChanged();
        }
    }

    public string RenderConnectionInfo => _connectionInfo?.RenderConnectionInfo() ?? string.Empty;

    private void NotifyConnectionInfoChanged()
    {
        OnPropertyChanged(nameof(RenderConnectionInfo));
        // Thông báo cho MainViewModel để cập nhật footer
        MainViewModel.Current?.UpdateConnectionInfo(RenderConnectionInfo);
    }

    #endregion

    #region Commands

    public ICommand CheckConnectionCommand { get; }
    public ICommand OpenConnectionFileCommand { get; }
    public ICommand ShareConnectionFileCommand { get; }
    public ICommand LoadConnectionCommand { get; }

    #endregion

    #region Private Methods

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
                    UpdateConnectionInfoProperties(); // Cập nhật properties với thông tin kết nối đã đọc
                    await _parentPage.DisplayAlert("Thông báo", "Đọc và kiểm tra thông tin kết nối đã lưu thành công!", "OK");
                    _connectionService.ConnectionInfo = _connectionInfo;
                    _connectionService.HasLoadedInitialConnection = true; // Đánh dấu đã load lần đầu
                    _lastValidConnectionInfo = CloneConnectionInfo(_connectionInfo);
                    IsShareButtonEnabled = true;
                }
                else
                {
                    await _parentPage.DisplayAlert("Lỗi",
                        $"Kết nối thất bại: {message}\nVui lòng sửa và kiểm tra lại thông tin kết nối!", "OK");
                    _connectionService.HasLoadedInitialConnection = true; // Đánh dấu đã thử load (thành công hay thất bại)
                }

                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc file cấu hình: {ex.Message}");
            _connectionService.HasLoadedInitialConnection = true; // Đánh dấu đã thử load (thành công hay thất bại)
        }

        // Nếu không đọc được từ file hoặc dữ liệu không hợp lệ, sử dụng giá trị mặc định
        if (!_connectionService.HasLoadedInitialConnection)
        {
            SetDefaultConnectionInfo();
            _connectionService.HasLoadedInitialConnection = true;
        }
    }
    private void UpdateConnectionInfoProperties()
    {
        Server = _connectionInfo.Server;
        Database = _connectionInfo.Database;
        UserId = _connectionInfo.Username;
        Password = _connectionInfo.Password;
        DvhcRootId = _connectionInfo.DvhcRootId; // Chuyển đổi sang chuỗi để hiển thị
        UpdateShareButtonState();
        NotifyConnectionInfoChanged();
    }
    private void SetDefaultConnectionInfo()
    {
        _connectionInfo = new ConnectionInfo
        {
            Server = "localhost",
            Database = "elis",
            Username = "sa",
            Password = "1234567",
            DvhcRootId = 27 // Mặc định là 27 (Bắc Ninh)
        };
        UpdateConnectionInfoProperties();
        _connectionService.ConnectionInfo = _connectionInfo;
        NotifyConnectionInfoChanged();
    }

    private async Task SaveConnection()
    {
        try
        {
            var (success, message) = await _connectionInfo.SaveConnectionInfoAsync();
            if (success)
            {
                _connectionService.ConnectionInfo = _connectionInfo;
                _lastValidConnectionInfo = CloneConnectionInfo(_connectionInfo);
                IsShareButtonEnabled = true;
            }
            else
            {
                await _parentPage.DisplayAlert("Lỗi", $"Không thể lưu thông tin kết nối:\n {message}", "OK");
            }
        }
        catch (Exception exception)
        {
            await _parentPage.DisplayAlert("Lỗi", $"Không thể kiểm tra kết nối: {exception.Message}", "OK");
        }
    }
    private void OnConnectionInfoChanged()
    {
        UpdateShareButtonState();
        NotifyConnectionInfoChanged();
    }

    private void UpdateShareButtonState()
    {
        if (_lastValidConnectionInfo == null)
        {
            IsShareButtonEnabled = false;
            return;
        }

        var currentInfo = new ConnectionInfo
        {
            Server = Server,
            Database = Database,
            Username = UserId,
            Password = Password,
            DvhcRootId = DvhcRootId,
        };

        bool isUnchanged = IsConnectionInfoEqual(currentInfo, _lastValidConnectionInfo);
        IsShareButtonEnabled = isUnchanged;
    }

    private static bool IsConnectionInfoEqual(ConnectionInfo? info1, ConnectionInfo? info2)
    {
        if (info1 == null || info2 == null) return false;

        return string.Equals(info1.Server, info2.Server, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(info1.Database, info2.Database, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(info1.Username, info2.Username, StringComparison.Ordinal) &&
               string.Equals(info1.Password, info2.Password, StringComparison.Ordinal);
    }

    private static ConnectionInfo CloneConnectionInfo(ConnectionInfo original)
    {
        return new ConnectionInfo
        {
            Server = original.Server,
            Database = original.Database,
            Username = original.Username,
            Password = original.Password,
            DvhcRootId = original.DvhcRootId,
            UseIntegratedSecurity = original.UseIntegratedSecurity,
            ConnectTimeout = original.ConnectTimeout,
            ExpiryDate = original.ExpiryDate
        };
    }

    #endregion

    #region Command Implementations

    private async Task CheckConnectionAsync()
    {
        try
        {
            IsShareButtonEnabled = false;

            // Cập nhật dữ liệu từ properties vào ConnectionInfo
            _connectionInfo.Server = Server;
            _connectionInfo.Database = Database;
            _connectionInfo.Username = UserId;
            _connectionInfo.Password = Password;
            _connectionInfo.DvhcRootId = DvhcRootId;

            var (success, message) = await _connectionInfo.CheckConnection(); if (success)
            {
                await _parentPage.DisplayAlert("ℹ Thông báo", "Kết nối thành công!", "OK");
                await SaveConnection();
                NotifyConnectionInfoChanged();
            }
            else
            {
                IsShareButtonEnabled = false;
                await _parentPage.DisplayAlert("⚠ Kết nối thất bại", message, "OK");
            }
        }
        catch (Exception exception)
        {
            await _parentPage.DisplayAlert("❌ Không thể kiểm tra kết nối", exception.Message, "OK");
        }
    }

    private async Task OpenConnectionFileAsync()
    {
        try
        {
            var pickOptions = new PickOptions
            {
                PickerTitle = "Chọn tệp kết nối để mở"
            };

            var result = await FilePicker.Default.PickAsync(pickOptions);
            if (result != null)
            {
                await ProcessConnectionOpen(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            if (ex.Message.Contains("This platform does not support this file type"))
                errorMessage = "Định dạng tệp không được hỗ trợ. Vui lòng chọn tệp .inf, .json hoặc .txt";
            else if (ex.Message.Contains("No file was selected"))
                return;

            await _parentPage.DisplayAlert("Không thể mở thông tin kết nối", errorMessage, "OK");
        }
    }

    private async Task ProcessConnectionOpen(string filePath)
    {
        try
        {
            var (openedConnection, _) = await filePath.ImportConnectionSettings(string.Empty);
            if (openedConnection is not null)
            {
                _connectionInfo = openedConnection;
                UpdateConnectionInfoProperties();
                await SaveConnection();
                await _parentPage.DisplayAlert("✅ Mở tệp thành công",
                    "Đã mở thông tin kết nối từ tệp không mã hóa thành công!", "OK");
                return;
            }
        }
        catch (Exception)
        {
            // Nếu thất bại, có thể tệp được mã hóa, yêu cầu nhập mật khẩu
        }

        await RequestPasswordAndOpen(filePath);
    }

    private async Task RequestPasswordAndOpen(string filePath)
    {
        ConnectionInfo? previewConnection = await TryGetConnectionPreview(filePath);

        while (true)
        {
            var secretKey = previewConnection != null
                ? await _parentPage.DisplayPasswordPromptAsync(
                    "🔒 Nhập mật khẩu giải mã",
                    "Tệp được mã hóa. Vui lòng nhập mật khẩu để giải mã tệp kết nối:",
                    previewConnection)
                : await _parentPage.DisplayPasswordPromptAsync(
                    "🔒 Nhập mật khẩu giải mã",
                    "Tệp được mã hóa. Vui lòng nhập mật khẩu để giải mã tệp kết nối:");

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                await _parentPage.DisplayAlert("Hủy mở tệp", "Bạn đã hủy việc mở tệp kết nối!", "OK");
                return;
            }

            var success = await TryOpenWithPassword(filePath, secretKey);
            if (success)
                break;
        }
    }

    private async Task<bool> TryOpenWithPassword(string filePath, string secretKey)
    {
        try
        {
            var (openedConnection, _) = await filePath.ImportConnectionSettings(secretKey);

            if (openedConnection is not null)
            {
                _connectionInfo = openedConnection;
                UpdateConnectionInfoProperties();
                await SaveConnection();
                await _parentPage.DisplayAlert("✅ Mở tệp thành công",
                    "Đã mở và giải mã thông tin kết nối thành công!", "OK");
                return true;
            }

            await _parentPage.DisplayAlert("❌ Mật khẩu không chính xác",
                "Không thể giải mã tệp với mật khẩu đã nhập.\nVui lòng kiểm tra lại mật khẩu!", "Thử lại");
            return false;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("password") || ex.Message.Contains("decrypt") || ex.Message.Contains("mã hóa"))
            {
                await _parentPage.DisplayAlert("❌ Mật khẩu không chính xác",
                    "Mật khẩu giải mã không đúng. Vui lòng thử lại!", "Thử lại");
                return false;
            }
            else
            {
                await _parentPage.DisplayAlert("❌ Lỗi mở tệp", $"Không thể đọc thông tin kết nối từ tệp:\n{ex.Message}", "OK");
                return false;
            }
        }
    }

    private async Task ShareConnectionFileAsync()
    {
        try
        {
            var sharePage = new ShareConnectionPage(_connectionInfo);
            await _parentPage.Navigation.PushModalAsync(sharePage);
        }
        catch (Exception ex)
        {
            await _parentPage.DisplayAlert("Lỗi", $"Không thể mở trang chia sẻ: {ex.Message}", "OK");
        }
    }

    private static async Task<ConnectionInfo?> TryGetConnectionPreview(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                return ConnectionInfo.DeserializeConnectionInfo(content, encrypted: false);
            }
            catch
            {
                return null;
            }
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
