using Haihv.Elis.Tools.Data.Models;

namespace Haihv.Elis.Tools.App.Controls;

public partial class OpenConnectionPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _taskCompletionSource;
    private readonly ConnectionInfo? _connectionInfo;

    public OpenConnectionPage(string title, string message)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        _taskCompletionSource = new TaskCompletionSource<string?>();

        // Focus vào password entry khi page được hiển thị
        this.Loaded += (s, e) => PasswordEntry.Focus();
    }

    public OpenConnectionPage(string title, string message, ConnectionInfo? connectionInfo) : this(title, message)
    {
        _connectionInfo = connectionInfo;
        CheckExpiryStatus();
    }
    public Task<string?> GetPasswordAsync()
    {
        return _taskCompletionSource.Task;
    }

    private void CheckExpiryStatus()
    {
        if (_connectionInfo?.ExpiryDate != null)
        {
            ExpiryInfoLabel.IsVisible = true;

            var now = DateTime.Now;
            var expiryDate = _connectionInfo.ExpiryDate.Value;
            var timeUntilExpiry = expiryDate - now;

            if (timeUntilExpiry.TotalDays < 0)
            {
                // Đã hết hạn
                ExpiryInfoLabel.Text = "⚠️ Tệp này đã hết hạn và không thể mở được";
                ExpiryInfoLabel.TextColor = Colors.Red;

                // Vô hiệu hóa các nút
                PasswordEntry.IsEnabled = false;
                OkButton.IsEnabled = false;
                OkButton.Text = "Đã hết hạn";
            }
            else if (timeUntilExpiry.TotalHours <= 24)
            {
                // Sắp hết hạn trong 24h
                var hoursLeft = (int)timeUntilExpiry.TotalHours;
                ExpiryInfoLabel.Text = $"⏰ Tệp sẽ hết hạn sau {hoursLeft} giờ";
                ExpiryInfoLabel.TextColor = Colors.Orange;
            }
            else
            {
                // Còn nhiều thời gian
                var daysLeft = (int)timeUntilExpiry.TotalDays;
                ExpiryInfoLabel.Text = $"📅 Tệp sẽ hết hạn sau {daysLeft} ngày";
                ExpiryInfoLabel.TextColor = Colors.Gray;
            }
        }
    }

    private void OnPasswordEntryCompleted(object sender, EventArgs e)
    {
        // Khi người dùng nhấn Enter
        OnOkClicked(sender, e);
    }
    private async void OnOkClicked(object sender, EventArgs e)
    {
        // Kiểm tra xem tệp có hết hạn không
        if (_connectionInfo?.ExpiryDate != null && !_connectionInfo.IsNotExpired())
        {
            await DisplayAlert("Tệp hết hạn", "Tệp này đã hết hạn và không thể mở được.", "OK");
            _taskCompletionSource.SetResult(null);
            await Navigation.PopModalAsync();
            return;
        }

        var password = PasswordEntry.Text;
        _taskCompletionSource.SetResult(password);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        _taskCompletionSource.SetResult(null);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        // Xử lý khi người dùng nhấn nút Back
        _taskCompletionSource.SetResult(null);
        _ = Navigation.PopModalAsync();
        return true;
    }
}
