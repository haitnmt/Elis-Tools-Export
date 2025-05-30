using Haihv.Elis.Tools.Export.App.Shared.Services;
using Haihv.Elis.Tools.Export.App.Shared.Settings;
using Haihv.Elis.Tools.Export.App.Web.Components;
using Haihv.Elis.Tools.Export.App.Web.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.FluentUI.AspNetCore.Components;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// Add device-specific services used by the Haihv.Elis.Tools.Export.App.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Haihv.Elis.Tools.Export.App.Shared._Imports).Assembly);

app.Run();
