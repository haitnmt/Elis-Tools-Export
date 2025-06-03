using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Maui.Extensions;
using Haihv.Extensions.String;

namespace Haihv.Elis.Tools.App.ContentPages;

public partial class ShareConnectionPage : ContentPage
{
    private readonly ConnectionInfo _connectionInfo = null!;
    private bool _isPasswordVisible = true;
    public ShareConnectionPage()
    {
        InitializeComponent();
        SetupEventHandlers();
        InitializeDefaultState();
    }

    public ShareConnectionPage(ConnectionInfo connectionInfo) : this()
    {
        _connectionInfo = connectionInfo;
    }
    private void InitializeDefaultState()
    {
        // Mặc định sử dụng mã hóa
        PasswordLayout.IsVisible = true;
        PasswordStrengthLayout.IsVisible = false;
        // Tạo vào hiển thị mật khẩu ngẫu nhiên
        PasswordEntry.IsPassword = !_isPasswordVisible;
        PasswordEntry.Text = EncryptionHelper.GenerateSecurePassword(16);
        UpdateTogglePasswordButton();

        // Thiết lập thời gian hết hạn mặc định (1 ngày)
        InitializeExpiryDate();
    }
    private void SetupEventHandlers()
    {
        NoEncryptCheckBox.CheckedChanged += NoEncryptCheckBox_CheckedChanged;
        PasswordEntry.TextChanged += PasswordEntry_TextChanged;
        ExpiryDatePicker.DateSelected += ExpiryDatePicker_DateSelected;
    }

    private void InitializeExpiryDate()
    {
        // Mặc định hết hạn sau 1 ngày
        ExpiryDatePicker.Date = DateTime.Now;
        ExpiryDatePicker.MinimumDate = DateTime.Now.AddDays(0); // Tối thiểu là ngày hôm nay
        ExpiryDatePicker.MaximumDate = DateTime.Now.AddDays(5); // Tối đa là 5 ngày
        UpdateExpiryInfo();
    }

    private void UpdateExpiryInfo()
    {
        var selectedDate = ExpiryDatePicker.Date;
        var daysFromNow = (selectedDate.Date - DateTime.Now.Date).Days;

        if (daysFromNow <= 0)
        {
            ExpiryInfoLabel.Text = "Tệp sẽ hết hạn ngay hôm nay";
            ExpiryInfoLabel.TextColor = Colors.Green;
        }
        else if (daysFromNow == 1)
        {
            ExpiryInfoLabel.Text = "Tệp sẽ hết hạn ngày mai";
            ExpiryInfoLabel.TextColor = Colors.Blue;
        }
        else
        {
            ExpiryInfoLabel.Text = $"Tệp sẽ hết hạn sau {daysFromNow} ngày";
            if (daysFromNow < 4)
            {
                ExpiryInfoLabel.TextColor = Colors.Orange;
            }
            else
            {
                ExpiryInfoLabel.TextColor = Colors.Red;
            }
        }
    }

    private void NoEncryptCheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Logic đảo ngược: khi checkbox "không mã hóa" được chọn thì ẩn phần mật khẩu
        PasswordLayout.IsVisible = !e.Value;
    }

    private void PasswordEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdatePasswordStrength(e.NewTextValue);
    }
    private void UpdatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            PasswordStrengthLayout.IsVisible = false;
            return;
        }

        PasswordStrengthLayout.IsVisible = true;

        try
        {
            int strength = EncryptionHelper.CheckPasswordStrength(password);
            double strengthPercent = strength / 100.0;

            PasswordStrengthBar.Progress = strengthPercent;

            // Cập nhật màu sắc và text dựa trên độ mạnh
            if (strength < 30)
            {
                PasswordStrengthLabel.Text = "Yếu";
                PasswordStrengthLabel.TextColor = Colors.Red;
                PasswordStrengthBar.ProgressColor = Colors.Red;
            }
            else if (strength < 60)
            {
                PasswordStrengthLabel.Text = "Trung bình";
                PasswordStrengthLabel.TextColor = Colors.Orange;
                PasswordStrengthBar.ProgressColor = Colors.Orange;
            }
            else if (strength < 80)
            {
                PasswordStrengthLabel.Text = "Mạnh";
                PasswordStrengthLabel.TextColor = Colors.YellowGreen;
                PasswordStrengthBar.ProgressColor = Colors.YellowGreen;
            }
            else
            {
                PasswordStrengthLabel.Text = "Rất mạnh";
                PasswordStrengthLabel.TextColor = Colors.Green;
                PasswordStrengthBar.ProgressColor = Colors.Green;
            }
        }
        catch
        {
            PasswordStrengthLayout.IsVisible = false;
        }
    }

    private void ExpiryDatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        UpdateExpiryInfo();
    }

    private void ResetExpiryBtn_Clicked(object? sender, EventArgs e)
    {
        ExpiryDatePicker.Date = DateTime.Now;
        UpdateExpiryInfo();
    }

    private async void GeneratePasswordBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            string newPassword = EncryptionHelper.GenerateSecurePassword(16);
            PasswordEntry.Text = newPassword;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể tạo mật khẩu: {ex.Message}", "OK");
        }
    }

    private void TogglePasswordBtn_Clicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        UpdateTogglePasswordButton();
    }
    private void UpdateTogglePasswordButton()
    {
        // Sử dụng emoji thay vì FontImageSource để đảm bảo hiển thị
        TogglePasswordBtn.Text = _isPasswordVisible ? "🙈" : "👁";
    }

    private async void CopyPasswordBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            string password = PasswordEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Thông báo", "Không có mật khẩu để copy", "OK");
                return;
            }

            await Clipboard.Default.SetTextAsync(password);

            // Hiển thị feedback ngắn
            var originalText = CopyPasswordBtn.Text;
            CopyPasswordBtn.Text = "✅";
            CopyPasswordBtn.IsEnabled = false;

            await DisplayAlert("Thành công", "Đã copy mật khẩu vào clipboard", "OK");
            CopyPasswordBtn.Text = originalText;
            CopyPasswordBtn.IsEnabled = true;

        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể copy mật khẩu: {ex.Message}", "OK");
        }
    }

    private async void SaveBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            SaveBtn.IsEnabled = false;
            SaveBtn.Text = "Đang xử lý...";

            string secretKey = string.Empty;

            // Logic mới: mặc định mã hóa, chỉ không mã hóa khi NoEncryptCheckBox được chọn
            if (!NoEncryptCheckBox.IsChecked)
            {
                secretKey = PasswordEntry.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    await DisplayAlert("Thông báo", "Vui lòng nhập mật khẩu hoặc tạo mật khẩu ngẫu nhiên!", "OK");
                    return;
                }
                if (secretKey.Length < 8)
                {
                    await DisplayAlert("Thông báo", "Mật khẩu phải có ít nhất 8 ký tự", "OK");
                    return;
                }
            }            // Đặt thời gian hết hạn cho connection info
            _connectionInfo.ExpiryDate = ExpiryDatePicker.Date;

            // Tạo tệp vào thư mục Downloads - mã hóa khi secretKey có giá trị, không mã hóa khi rỗng
            var (success, filePath) = string.IsNullOrEmpty(secretKey)
                ? await _connectionInfo.ExportConnectionSettings()
                : await _connectionInfo.ExportConnectionSettings(secretKey);

            if (!success)
            {
                await DisplayAlert("Lỗi", $"Không thể tạo tệp: {filePath}", "OK");
                return;
            }

            // Hiển thị kết quả
            ShowResult(filePath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Có lỗi xảy ra: {ex.Message}", "OK");
        }
        finally
        {
            SaveBtn.IsEnabled = true;
            SaveBtn.Text = "Lưu tệp";
        }
    }
    private void ShowResult(string filePath)
    {
        // Ẩn form nhập liệu và nút điều khiển
        EncryptionOptionsLayout.IsVisible = false;
        PasswordLayout.IsVisible = false;
        ExpiryLayout.IsVisible = false;
        ButtonsLayout.IsVisible = false;

        // Hiển thị kết quả
        ResultLayout.IsVisible = true;
        FilePathLabel.Text = filePath;
    }

    private async void OpenLocationBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            string folderPath = Path.GetDirectoryName(FilePathLabel.Text) ?? "";
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                await Launcher.Default.OpenAsync(new Uri($"file://{folderPath}"));
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"Không thể mở thư mục: {ex.Message}", "OK");
        }
    }

    private async void CloseBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void CancelBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        // Xử lý khi người dùng nhấn nút Back
        _ = Navigation.PopModalAsync();
        return true;
    }
}
