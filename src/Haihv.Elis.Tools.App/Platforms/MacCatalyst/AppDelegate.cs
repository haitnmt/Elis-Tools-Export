using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Haihv.Elis.Tools.App;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp();
    }
}