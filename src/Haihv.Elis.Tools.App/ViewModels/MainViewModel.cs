using Haihv.Elis.Tools.App.ContentViews;
using Haihv.Elis.Tools.App.Models;
using Haihv.Elis.Tools.App.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Haihv.Elis.Tools.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private View? _currentContent;
    private string _renderConnectionInfo = string.Empty;

    public ObservableCollection<MenuToolbarItem> ToolbarItems { get; } = new();

    public View? CurrentContent
    {
        get => _currentContent;
        set
        {
            _currentContent = value;
            OnPropertyChanged(nameof(CurrentContent));
            System.Diagnostics.Debug.WriteLine($"CurrentContent set to: {value?.GetType().Name ?? "null"}");
        }
    }

    public string RenderConnectionInfo
    {
        get => _renderConnectionInfo;
        set
        {
            _renderConnectionInfo = value;
            OnPropertyChanged(nameof(RenderConnectionInfo));
        }
    }
    public ICommand ShowConnectionSettingCommand { get; }
    public ICommand ShowExportDataToXmlCommand { get; }

    // Static instance để có thể access từ ConnectionSettingViewModel
    public static MainViewModel? Current { get; private set; }
    public MainViewModel()
    {
        Current = this;

        ShowConnectionSettingCommand = new Command(() =>
        {
            System.Diagnostics.Debug.WriteLine("Show ConnectionSetting executed");
            LoadConnectionSettingPage();
        });

        ShowExportDataToXmlCommand = new Command(() =>
        {
            System.Diagnostics.Debug.WriteLine("Show ExportDataToXml executed");
            LoadExportDataToXmlPage();
        });        // Khởi tạo thông tin kết nối mặc định
        RenderConnectionInfo = "localhost/elis (sa)";

        // Khởi tạo toolbar items
        InitializeToolbarItems();

        // Khởi tạo mặc định hiển thị Page1
        InitializeDefaultPage();
    }
    public void UpdateConnectionInfo(string connectionInfo)
    {
        RenderConnectionInfo = connectionInfo;
    }
    private void InitializeToolbarItems()
    {
        ToolbarItems.Add(new MenuToolbarItem
        {
            Icon = "🔐",
            Title = "Cấu hình kết nối",
            IconColor = "#88C0D0",
            Command = ShowConnectionSettingCommand,
            IsActive = true
        });

        ToolbarItems.Add(new MenuToolbarItem
        {
            Icon = "📑",
            Title = "Trích xuất dữ liệu XML VBDLIS",
            IconColor = "#A3BE8C",
            Command = ShowExportDataToXmlCommand,
            IsActive = false
        });
    }

    private void InitializeDefaultPage()
    {
        // Sử dụng Dispatcher để đảm bảo resources đã sẵn sàng
        Dispatcher.GetForCurrentThread()?.Dispatch(LoadConnectionSettingPage);
    }
    private void LoadConnectionSettingPage()
    {
        try
        {
            var connectionSetting = new ConnectionSetting();
            CurrentContent = connectionSetting;
            System.Diagnostics.Debug.WriteLine("ConnectionSetting created and set");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating ConnectionSetting: {ex.Message}");
        }
    }

    private void LoadExportDataToXmlPage()
    {
        try
        {
            var exportDataToXml = new ExportDataToXmlVBDLIS();
            CurrentContent = exportDataToXml;
            System.Diagnostics.Debug.WriteLine("ExportDataToXml created and set");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating ExportDataToXml: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}