using Microsoft.Extensions.Logging;
using SchoolConnect.Shared.Services;

namespace SchoolConnect.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        using var configStream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
        builder.Configuration.AddJsonStream(configStream);

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<SchoolContentStore>();
        builder.Services.AddScoped<SchoolContentService>();
        builder.Services.AddScoped<PortalSessionService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
