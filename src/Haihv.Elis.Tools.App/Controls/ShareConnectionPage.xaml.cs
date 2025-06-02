using Haihv.Elis.Tools.Data.Models;

namespace Haihv.Elis.Tools.App.Controls;

public partial class ShareConnectionPage : ContentPage
{
    public ShareConnectionPage()
    {
        InitializeComponent();
        SetupEventHandlers();
    }
    public ShareConnectionPage(ConnectionInfo connectionInfo) : this()
    {
        ShareControl.SetConnectionInfo(connectionInfo);
    }

    private void SetupEventHandlers()
    {
        ShareControl.ShareCompleted += ShareControl_ShareCompleted;
        ShareControl.Cancelled += ShareControl_Cancelled;
        ShareControl.ErrorOccurred += ShareControl_ErrorOccurred;
    }

    private async void ShareControl_ErrorOccurred(object? sender, string errorMessage)
    {
        await DisplayAlert("Lỗi", errorMessage, "OK");
    }

    private async void ShareControl_ShareCompleted(object? sender, ShareConnectionEventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void ShareControl_Cancelled(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
