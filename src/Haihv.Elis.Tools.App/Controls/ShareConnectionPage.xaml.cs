using Haihv.Elis.Tools.Data.Models;
using Haihv.Elis.Tools.Maui.Extensions;
using System.Text;

namespace Haihv.Elis.Tools.App.Controls;

public partial class ShareConnectionPage : ContentPage
{
    private ConnectionInfo _connectionInfo = null!;
    private string _generatedPassword = string.Empty;

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
        // Mặc định sử dụng mã hóa, hiển thị phần mật khẩu
        PasswordLayout.IsVisible = true;
        UpdatePasswordLayoutVisibility();

        // Tự động tạo password khi khởi tạo
        if (AutoPasswordRadio.IsChecked)
        {
            GeneratePassword();
        }
    }
    private void SetupEventHandlers()
    {
        NoEncryptCheckBox.CheckedChanged += NoEncryptCheckBox_CheckedChanged;
        AutoPasswordRadio.CheckedChanged += PasswordRadio_CheckedChanged;
        ManualPasswordRadio.CheckedChanged += PasswordRadio_CheckedChanged;
    }

    private void NoEncryptCheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        // Logic đảo ngược: khi checkbox "không mã hóa" được chọn thì ẩn phần mật khẩu
        PasswordLayout.IsVisible = !e.Value;
        UpdatePasswordLayoutVisibility();

        // Tự động tạo password khi bỏ chọn "không mã hóa" (tức là sẽ mã hóa) và chọn auto
        if (!e.Value && AutoPasswordRadio.IsChecked)
        {
            GeneratePassword();
        }
    }

    private void PasswordRadio_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        UpdatePasswordLayoutVisibility();

        // Tự động tạo password khi chọn AutoPasswordRadio
        if (AutoPasswordRadio.IsChecked && e.Value)
        {
            GeneratePassword();
        }
    }
    private void UpdatePasswordLayoutVisibility()
    {
        // Logic đảo ngược: nếu checkbox "không mã hóa" được chọn thì ẩn các option mật khẩu
        if (NoEncryptCheckBox.IsChecked)
        {
            AutoPasswordLayout.IsVisible = false;
            ManualPasswordEntry.IsVisible = false;
            return;
        }

        AutoPasswordLayout.IsVisible = AutoPasswordRadio.IsChecked;
        ManualPasswordEntry.IsVisible = ManualPasswordRadio.IsChecked;
    }

    private void GeneratePassword()
    {
        try
        {
            _generatedPassword = GenerateRandomPassword();
            AutoPasswordEntry.Text = _generatedPassword;
        }
        catch (Exception ex)
        {
            _ = DisplayAlert("Lỗi", $"Không thể tạo mật khẩu: {ex.Message}", "OK");
        }
    }

    private async void CopyPasswordBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_generatedPassword))
            {
                GeneratePassword();
            }

            await Clipboard.Default.SetTextAsync(_generatedPassword);

            // Hiển thị feedback ngắn
            var originalText = CopyPasswordBtn.Text;
            CopyPasswordBtn.Text = "✅ Đã copy";
            CopyPasswordBtn.IsEnabled = false;

            await Task.Delay(1500);

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
            SaveBtn.Text = "Đang xử lý..."; string secretKey = string.Empty;

            // Logic mới: mặc định mã hóa, chỉ không mã hóa khi NoEncryptCheckBox được chọn
            if (!NoEncryptCheckBox.IsChecked)
            {
                if (AutoPasswordRadio.IsChecked)
                {
                    if (string.IsNullOrWhiteSpace(_generatedPassword))
                    {
                        // Tự động tạo password nếu chưa có
                        GeneratePassword();
                    }
                    secretKey = _generatedPassword;
                }
                else
                {
                    secretKey = ManualPasswordEntry.Text?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(secretKey))
                    {
                        await DisplayAlert("Lỗi", "Vui lòng nhập mật khẩu!", "OK");
                        return;
                    }
                }
            }            // Tạo tệp vào thư mục Downloads - mã hóa khi secretKey có giá trị, không mã hóa khi rỗng
            var (success, filePath) = string.IsNullOrEmpty(secretKey)
                ? await _connectionInfo.ExportConnectionSettings()
                : await _connectionInfo.ExportConnectionSettings(secretKey);

            if (!success)
            {
                await DisplayAlert("Lỗi", $"Không thể tạo tệp: {filePath}", "OK");
                return;
            }

            // Hiển thị kết quả
            ShowResult(filePath, !string.IsNullOrEmpty(secretKey));
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
    private void ShowResult(string filePath, bool isEncrypted)
    {
        // Ẩn form nhập liệu và nút điều khiển
        EncryptionOptionsLayout.IsVisible = false;
        PasswordLayout.IsVisible = false;
        ButtonsLayout.IsVisible = false;

        // Hiển thị kết quả
        FilePathLabel.Text = filePath;
        ResultLayout.IsVisible = true;
    }

    private async void OpenLocationBtn_Clicked(object sender, EventArgs e)
    {
        try
        {
            await FileHelper.OpenFileLocation(FilePathLabel.Text);
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

    private string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
        var stringBuilder = new StringBuilder();
        var random = Random.Shared;

        for (int i = 0; i < 16; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }

    protected override bool OnBackButtonPressed()
    {
        // Xử lý khi người dùng nhấn nút Back
        _ = Navigation.PopModalAsync();
        return true;
    }
}
