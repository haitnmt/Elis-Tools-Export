namespace Haihv.Elis.Tools.Export.Services;

public static class ServiceHelper
{
    public static TService GetService<TService>()
        where TService : class
    {
        return Current.GetService<TService>() ?? throw new InvalidOperationException($"Service {typeof(TService)} not found");
    }

    private static IServiceProvider Current
    {
        get
        {
#if WINDOWS10_0_17763_0_OR_GREATER
            return MauiWinUIApplication.Current?.Services ?? throw new InvalidOperationException("MauiWinUIApplication.Current is null");
#elif MACCATALYST
            return IPlatformApplication.Current?.Services ?? throw new InvalidOperationException("IPlatformApplication.Current is null");
#else
            throw new PlatformNotSupportedException();
#endif
        }
    }
}
