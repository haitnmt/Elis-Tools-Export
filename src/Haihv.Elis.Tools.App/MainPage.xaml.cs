using Haihv.Elis.Tools.App.ContentViews;
using Haihv.Elis.Tools.App.Models;
using Haihv.Elis.Tools.App.ViewModels;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.App.Views;

namespace Haihv.Elis.Tools.App;

public partial class MainPage : ContentPage
{
    private readonly ConnectionService _connectionService; 
    public MainPage(ConnectionService connectionService)
    {
        InitializeComponent();
        _connectionService = connectionService;

        // Khởi tạo ViewModel cho MainPage
        BindingContext = new MainViewModel();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Khởi tạo ConnectionSetting với dependencies khi page xuất hiện
        InitializeConnectionSetting();
    }
    private void InitializeConnectionSetting()
    {
        try
        {
            // Kiểm tra xem content hiện tại có phải là ConnectionSetting không
            if (BindingContext is not MainViewModel viewModel ||
                viewModel.CurrentContent == null) return;
            // Nếu template hiện tại là ConnectionSettingPage (ConnectionSetting)
            var currentTemplate = viewModel.CurrentContent;
            if (Application.Current?.Resources != null &&
                Application.Current.Resources.TryGetValue("ConnectionSettingPage", out var connectionSettingPage) &&
                currentTemplate == connectionSettingPage)
            {
                // Tìm ConnectionSetting instance trong visual tree
                InitializeConnectionSettingInTemplate();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi khi khởi tạo ConnectionSetting: {ex.Message}");
        }
    }

    private void InitializeConnectionSettingInTemplate()
    {        // Sử dụng timer để đợi UI render xong
        Microsoft.Maui.Dispatching.Dispatcher.GetForCurrentThread()?.Dispatch(() =>
        {
            try
            {
                var connectionSetting = FindConnectionSettingInVisualTree(DynamicContent);
                connectionSetting?.Initialize(_connectionService, this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tìm ConnectionSetting: {ex.Message}");
            }
        });
    }

    private static ConnectionSetting? FindConnectionSettingInVisualTree(Element element)
    {
        while (true)
        {
            switch (element)
            {
                case ConnectionSetting connectionSetting:
                    return connectionSetting;
                case ContentView { Content: not null } contentView:
                    element = contentView.Content;
                    continue;
                case ContentPresenter { Content: Element content }:
                    element = content;
                    continue;
            }

            if (element is not Layout layout) return null;
            foreach (var child in layout.Children)
            {
                if (child is not Element childElement) continue;
                var result = FindConnectionSettingInVisualTree(childElement);
                if (result != null) return result;
            }

            return null;
        }
    }
}

