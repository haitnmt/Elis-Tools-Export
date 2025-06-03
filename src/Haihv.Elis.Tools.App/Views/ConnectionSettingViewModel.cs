using System.ComponentModel;
using System.Windows.Input;
using Haihv.Elis.Tools.App.Extensions;
using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.Maui.Extensions;

namespace Haihv.Elis.Tools.App.Views;

public class ConnectionSettingViewModel : INotifyPropertyChanged
{
    private ConnectionInfo _connectionInfo = null!;
    private ConnectionInfo? _lastValidConnectionInfo = null;
    private readonly ConnectionService _connectionService;
    private readonly Page _parentPage;

    private string _server = "localhost";
    private string _database = "elis";
    private string _userId = "sa";
    private string _password = "1234567";
    private bool _isShareButtonEnabled = false;

    public ConnectionSettingViewModel(ConnectionService connectionService, Page parentPage)
    {
        _connectionService = connectionService;
        _parentPage = parentPage;
    
        CheckConnectionCommand = new Command(async void () => await CheckConnectionAsync());
        OpenConnectionFileCommand = new Command(async void () => await OpenConnectionFileAsync());
        ShareConnectionFileCommand = new Command(async void () => await ShareConnectionFileAsync());

        // Khởi tạo thông tin kết nối mặc định
        SetDefaultConnectionInfo();

        // Đọc thông tin kết nối từ file cấu hình
        _ = LoadConnectionInfoAsync();
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

    #endregion

    #region Commands

    public ICommand CheckConnectionCommand { get; }
    public ICommand OpenConnectionFileCommand { get; }
    public ICommand ShareConnectionFileCommand { get; }

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
                var (success, message) = await _connectionInfo.CheckConnection();

                if (success)
                {
                    UpdateConnectionInfoProperties(); // Cập nhật properties với thông tin kết nối đã đọc
                    await _parentPage.DisplayAlert("Thông báo", "Đọc và kiểm tra thông tin kết nối đã lưu thành công!", "OK");
                    _connectionService.ConnectionInfo = _connectionInfo;
                    _lastValidConnectionInfo = CloneConnectionInfo(_connectionInfo);
                    IsShareButtonEnabled = true;
                }
                else
                {
                    await _parentPage.DisplayAlert("Lỗi",
                        $"Kết nối thất bại: {message}\nVui lòng sửa và kiểm tra lại thông tin kết nối!", "OK");
                }

                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc file cấu hình: {ex.Message}");
        }

        // Nếu không đọc được từ file hoặc dữ liệu không hợp lệ, sử dụng giá trị mặc định
        SetDefaultConnectionInfo();
    }

    private void UpdateConnectionInfoProperties()
    {
        Server = _connectionInfo.Server;
        Database = _connectionInfo.Database;
        UserId = _connectionInfo.Username;
        Password = _connectionInfo.Password;
        UpdateShareButtonState();
    }

    private void SetDefaultConnectionInfo()
    {
        _connectionInfo = new ConnectionInfo
        {
            Server = "localhost",
            Database = "elis",
            Username = "sa",
            Password = "1234567"
        };
        UpdateConnectionInfoProperties();
        _connectionService.ConnectionInfo = _connectionInfo;
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
            Server = Server ?? string.Empty,
            Database = Database ?? string.Empty,
            Username = UserId ?? string.Empty,
            Password = Password ?? string.Empty
        };

        bool isUnchanged = IsConnectionInfoEqual(currentInfo, _lastValidConnectionInfo);
        IsShareButtonEnabled = isUnchanged;
    }

    private static bool IsConnectionInfoEqual(ConnectionInfo info1, ConnectionInfo info2)
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

            var (success, message) = await _connectionInfo.CheckConnection();

            if (success)
            {
                await _parentPage.DisplayAlert("Thông báo", "Kết nối thành công!", "OK");
                await SaveConnection();
            }
            else
            {
                IsShareButtonEnabled = false;
                await _parentPage.DisplayAlert("Lỗi", $"Kết nối thất bại: {message}", "OK");
            }
        }
        catch (Exception exception)
        {
            await _parentPage.DisplayAlert("Lỗi", $"Không thể kiểm tra kết nối: {exception.Message}", "OK");
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

            await _parentPage.DisplayAlert("Lỗi mở tệp", $"Không thể mở thông tin kết nối:\n{errorMessage}", "OK");
        }
    }

    private async Task ProcessConnectionOpen(string filePath)
    {
        try
        {
            var (openedConnection, message) = await filePath.ImportConnectionSettings(string.Empty);
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
            var (openedConnection, message) = await filePath.ImportConnectionSettings(secretKey);

            if (openedConnection is not null)
            {
                _connectionInfo = openedConnection;
                UpdateConnectionInfoProperties();
                await SaveConnection();
                await _parentPage.DisplayAlert("✅ Mở tệp thành công",
                    "Đã mở và giải mã thông tin kết nối thành công!", "OK");
                return true;
            }
            else
            {
                await _parentPage.DisplayAlert("❌ Mật khẩu không chính xác",
                    "Không thể giải mã tệp với mật khẩu đã nhập.\nVui lòng kiểm tra lại mật khẩu!", "Thử lại");
                return false;
            }
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
            var sharePage = new Controls.ShareConnectionPage(_connectionInfo);
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

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
