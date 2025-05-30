using Haihv.Elis.Tools.Export.App.Services;
using Haihv.Elis.Tools.Export.App.Shared.Services;
using Haihv.Elis.Tools.Export.App.Shared.Settings;
using Haihv.Elis.Tools.Export.App.Web.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;

namespace Haihv.Elis.Tools.Export.App
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
                });

            // Add device-specific services used by the Haihv.Elis.Tools.Export.App.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddFluentUIComponents();
            // Đăng ký FileService
            builder.Services.AddSingleton<IFileService, FileService>();
            // Đăng ký Serilog (Write to File)
            var logger = new LoggerConfiguration()
                .WriteTo.File(
                    FilePath.LogFile($"Log_{DateTime.Now:yyyyMMdd_HHmmss}.log"),
                    rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .Enrich.FromLogContext()
                .CreateLogger();
            builder.Services.AddSerilog(logger);

            builder.Services.AddSingleton<IDistributedCache>(sp =>
                       new FileDistributedCache(
                           sp.GetRequiredService<IFileService>(),
                           FilePath.CacheOnDisk));
            builder.Services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = 1024 * 1024;
                options.MaximumKeyLength = 1024;
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(60),
                    LocalCacheExpiration = TimeSpan.FromDays(1)
                };
            });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
