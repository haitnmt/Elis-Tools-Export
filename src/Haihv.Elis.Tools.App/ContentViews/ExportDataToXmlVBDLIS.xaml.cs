using Haihv.Elis.Tools.App.ViewModels;
using Haihv.Elis.Tools.Data.Services;

namespace Haihv.Elis.Tools.App.Views;

public partial class ExportDataToXmlVBDLIS : ContentView
{
    private ExportDataToXmlVBDLISViewModel? _viewModel;
    public ExportDataToXmlVBDLIS()
    {
        InitializeComponent();
        // Delay initialization để đảm bảo Parent đã được set
        Loaded += OnLoaded;
    }
    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            Loaded -= OnLoaded; // Chỉ chạy một lần

            // Đợi một chút để UI hoàn tất việc load
            await Task.Delay(50);

            // Tự động khởi tạo với service từ DI container
            TryAutoInitialize();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: OnLoaded failed: {exception.Message}");
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: Stack trace: {exception.StackTrace}");
        }
    }
    private void TryAutoInitialize()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ConnectionSetting: Starting auto-initialization...");

            // Lấy service từ DI container
            var serviceProvider = IPlatformApplication.Current?.Services;
            if (serviceProvider == null)
            {
                System.Diagnostics.Debug.WriteLine("ConnectionSetting: ServiceProvider is null");
                return;
            }

            var connectionService = serviceProvider.GetService<ConnectionService>();
            if (connectionService == null)
            {
                System.Diagnostics.Debug.WriteLine("ConnectionSetting: ConnectionService not found in DI");
                return;
            }

            System.Diagnostics.Debug.WriteLine("ConnectionSetting: ConnectionService found");

            // Tìm parent page
            var parentPage = FindParentPage();
            if (parentPage == null)
            {
                System.Diagnostics.Debug.WriteLine("ConnectionSetting: Parent page not found");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: Parent page found: {parentPage.GetType().Name}");

            Initialize(connectionService, parentPage);
            System.Diagnostics.Debug.WriteLine("ConnectionSetting: Auto-initialization completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: Auto-initialize failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: Stack trace: {ex.StackTrace}");
        }
    }
    private Page? FindParentPage()
    {
        var parent = this.Parent;
        while (parent != null)
        {
            if (parent is Page page)
                return page;
            parent = parent.Parent;
        }
        return null;
    }
    public void Initialize(ConnectionService connectionService, Page parentPage)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ConnectionSetting: Creating ViewModel...");
            _viewModel = new ExportDataToXmlVBDLISViewModel(connectionService, parentPage);
            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: ViewModel created and BindingContext set. ViewModel type: {_viewModel.GetType().Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: Initialize failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"ConnectionSetting: Stack trace: {ex.StackTrace}");
        }
    }
}