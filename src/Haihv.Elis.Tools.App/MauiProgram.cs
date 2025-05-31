using Haihv.Elis.Tools.App.Settings;
using Microsoft.Extensions.Logging;
using Serilog;
using CommunityToolkit.Maui;

namespace Haihv.Elis.Tools.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder(); builder
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
                .WriteTo.File(FilePath.LogFile("app.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Logging.AddSerilog(Log.Logger);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
