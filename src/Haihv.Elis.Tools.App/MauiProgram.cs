using Microsoft.Extensions.Logging;
using Serilog;
using CommunityToolkit.Maui;
using Haihv.Elis.Tools.Data.Services;
using Haihv.Elis.Tools.Maui.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace Haihv.Elis.Tools.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Cấu hình Serilog để ghi log vào file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(FileHelper.LogFile("app.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger);
        builder.Services.AddSingleton<ConnectionService>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<ExportDataToXml>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}