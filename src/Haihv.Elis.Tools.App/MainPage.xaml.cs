using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.Maui.Extensions;
using Haihv.Elis.Tools.App.Extensions;
using System.Security.Cryptography;
using System.Text;

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
                PickerTitle = "Chọn file thông tin kết nối để import"
            };

            var result = await FilePicker.Default.PickAsync(pickOptions);
            if (result != null)
            {
                // Thử import mà không cần mật khẩu trước
                await ProcessConnectionImport(result.FullPath);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            // Xử lý các lỗi cụ thể
            if (ex.Message.Contains("This platform does not support this file type"))
                errorMessage = "Định dạng file không được hỗ trợ. Vui lòng chọn file .inf, .json hoặc .txt";
            else if (ex.Message.Contains("No file was selected"))
                // Người dùng hủy chọn file, không cần hiển thị thông báo lỗi
                return;

            await DisplayAlert("Lỗi import", $"Không thể import thông tin kết nối:\n{errorMessage}", "OK");
        }
    }

    private async Task ProcessConnectionImport(string filePath)
    {
        try
        {
            // Thử import mà không cần mật khẩu trước (file không mã hóa)
            var (importedConnection, message) = await filePath.ImportConnectionSettings(string.Empty);
            if (importedConnection is not null)
            {
                // Import thành công mà không cần mật khẩu
                _connectionInfo = importedConnection;
                UpdateConnectionInfoUi();
                await SaveConnection();
                await DisplayAlert("✅ Import thành công", 
                    "Đã import thông tin kết nối từ file không mã hóa thành công!", "OK");
                return;
            }
        }
        catch (Exception)
        {
            // Nếu thất bại, có thể file được mã hóa, yêu cầu nhập mật khẩu
        }

        // Nếu không import được mà không có mật khẩu, file có thể được mã hóa
        await RequestPasswordAndImport(filePath);
    }

    private async Task RequestPasswordAndImport(string filePath)
    {
        while (true)
        {
            // Yêu cầu nhập mật khẩu mã hóa với ký tự ẩn (****)
            var secretKey = await this.DisplayPasswordPromptAsync(
                "🔒 Nhập mật khẩu giải mã", 
                "File được mã hóa. Vui lòng nhập mật khẩu để giải mã file kết nối:");
            
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                await DisplayAlert("Hủy import", "Bạn đã hủy việc import file kết nối!", "OK");
                return;
            }
            
            // Thử import với mật khẩu đã nhập
            var success = await TryImportWithPassword(filePath, secretKey);
            if (success)
                break;
            
            // Nếu mật khẩu sai, tiếp tục vòng lặp để nhập lại
        }
    }

    private async Task<bool> TryImportWithPassword(string filePath, string secretKey)
    {
        try
        {
            // Sử dụng mật khẩu mã hóa khi import thông tin kết nối
            var (importedConnection, message) = await filePath.ImportConnectionSettings(secretKey);
            if (importedConnection is not null)
            {
                _connectionInfo = importedConnection;
                // Cập nhật UI với thông tin kết nối đã import
                UpdateConnectionInfoUi();
                await SaveConnection();
                
                await DisplayAlert("✅ Import thành công",
                    "Đã import và giải mã thông tin kết nối thành công!", "OK");
                return true;
            }
            else
            {
                await DisplayAlert("❌ Mật khẩu không chính xác", 
                    "Không thể giải mã file với mật khẩu đã nhập.\nVui lòng kiểm tra lại mật khẩu!", "Thử lại");
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
                await DisplayAlert("❌ Lỗi import", $"Không thể đọc thông tin kết nối từ file:\n{ex.Message}", "OK");
                return false;
            }
        }
    }

    private async void ExportDataBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            string secretKey = string.Empty;
            
            // Hiển thị dialog để tùy chọn mật khẩu
            var passwordChoice = await DisplayAlert("Tùy chọn bảo mật", 
                "Bạn có muốn mã hóa file kết nối để bảo mật thông tin không?", 
                "Có, mã hóa file", "Không, xuất bình thường");
            
            if (passwordChoice)
            {
                // Người dùng chọn tạo mật khẩu - hiển thị dialog tạo mật khẩu
                var createPasswordChoice = await DisplayAlert("Chọn cách tạo mật khẩu", 
                    "Bạn muốn tự nhập mật khẩu hay để hệ thống tạo ngẫu nhiên?", 
                    "Tạo ngẫu nhiên", "Tự nhập");
                
                if (createPasswordChoice)
                {
                    // Tạo mật khẩu ngẫu nhiên
                    secretKey = GenerateRandomPassword();
                    
                    // Hiển thị mật khẩu để người dùng biết và lưu lại
                    await DisplayAlert("Mật khẩu được tạo", 
                        $"Mật khẩu mã hóa đã được tạo:\n\n{secretKey}\n\n⚠️ Hãy lưu lại mật khẩu này để sử dụng khi import file!", 
                        "Đã lưu");
                }
                else
                {
                    // Yêu cầu người dùng nhập mật khẩu
                    secretKey = await DisplayPromptAsync("Tạo mật khẩu mã hóa", 
                        "Vui lòng nhập mật khẩu để mã hóa file:", 
                        "Mã hóa", "Hủy", 
                        placeholder: "Nhập mật khẩu mã hóa");
                    
                    if (string.IsNullOrWhiteSpace(secretKey))
                    {
                        await DisplayAlert("Hủy mã hóa", "Bạn đã hủy việc tạo mật khẩu. File sẽ được xuất không mã hóa.", "OK");
                        secretKey = string.Empty;
                    }
                }
            }
            
            // Lưu thông tin kết nối vào file (có mã hóa nếu người dùng đã nhập mật khẩu)
            var (success, message) = string.IsNullOrEmpty(secretKey) 
                ? await _connectionInfo.ExportConnectionSettings() 
                : await _connectionInfo.ExportConnectionSettings(secretKey);

            if (!success)
            {
                await DisplayAlert("Lỗi export", $"Không thể xuất file:\n{message}", "OK");
                return;
            }

            // Thông báo lưu thành công và hỏi có muốn chia sẻ không
            var exportMessage = string.IsNullOrEmpty(secretKey) 
                ? "Xuất file thành công!" 
                : "Xuất file đã mã hóa thành công!";
                
            var shareChoice = await DisplayAlert("Export thành công",
                $"{exportMessage}\n\nVị trí file: {message}\n\nBạn có muốn chia sẻ file này không?",
                "Chia sẻ", "Mở thư mục");

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
            await DisplayAlert("Lỗi export", $"Không thể export thông tin kết nối:\n{ex.Message}", "OK");
        }
    }

    // Phương thức tạo mật khẩu ngẫu nhiên
    private string GenerateRandomPassword()
    {
        // Tạo một chuỗi mật khẩu ngẫu nhiên 16 ký tự gồm chữ và số
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder stringBuilder = new();
        var random = Random.Shared;
        
        for (int i = 0; i < 16; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }
        
        return stringBuilder.ToString();
    }
}

