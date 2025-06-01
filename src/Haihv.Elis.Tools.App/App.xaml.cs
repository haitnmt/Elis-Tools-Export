using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Haihv.Elis.Tools.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Lấy tỷ lệ thu phóng của màn hình:
        var scale = DeviceDisplay.MainDisplayInfo.Density;
        return new Window(new AppShell())
        {
            Height = 750 * scale,
            Width = 960 * scale,
            MinimumHeight = 750,
            MinimumWidth = 960
        };
    }
}