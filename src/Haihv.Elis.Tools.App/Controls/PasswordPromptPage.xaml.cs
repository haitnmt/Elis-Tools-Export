namespace Haihv.Elis.Tools.App.Controls;

public partial class PasswordPromptPage : ContentPage
{
    private TaskCompletionSource<string?> _taskCompletionSource;

    public PasswordPromptPage(string title, string message)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
        _taskCompletionSource = new TaskCompletionSource<string?>();
        
        // Focus vào password entry khi page được hiển thị
        this.Loaded += (s, e) => PasswordEntry.Focus();
    }

    public Task<string?> GetPasswordAsync()
    {
        return _taskCompletionSource.Task;
    }

    private void OnPasswordEntryCompleted(object sender, EventArgs e)
    {
        // Khi người dùng nhấn Enter
        OnOkClicked(sender, e);
    }

    private async void OnOkClicked(object sender, EventArgs e)
    {
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
