using System.ComponentModel;
using System.Windows.Input;

namespace Haihv.Elis.Tools.App.Views;

public class MainViewModel : INotifyPropertyChanged
{
    private View? _currentContent;

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

    public ICommand ShowConnectionSettingCommand { get; }
    public ICommand ShowExportDataToXmlCommand { get; }
    public MainViewModel()
    {
        ShowConnectionSettingCommand = new Command(() =>
        {
            System.Diagnostics.Debug.WriteLine("Show ConnectionSetting executed");
            LoadConnectionSettingPage();
        });

        ShowExportDataToXmlCommand = new Command(() =>
        {
            System.Diagnostics.Debug.WriteLine("Show ExportDataToXml executed");
            LoadExportDataToXmlPage();
        });

        // Khởi tạo mặc định hiển thị Page1
        InitializeDefaultPage();
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
            var exportDataToXml = new ExportDataToXml();
            CurrentContent = exportDataToXml;
            System.Diagnostics.Debug.WriteLine("ExportDataToXml created and set");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating ExportDataToXml: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}