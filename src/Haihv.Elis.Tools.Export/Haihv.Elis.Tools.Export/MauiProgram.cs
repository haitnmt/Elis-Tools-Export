using Microsoft.Extensions.Logging;
using Haihv.Elis.Tools.Export.Services;

namespace Haihv.Elis.Tools.Export
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Đăng ký services
            builder.Services.AddSingleton<IConnectionService, ConnectionService>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
