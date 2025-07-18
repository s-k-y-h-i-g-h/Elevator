using Microsoft.Extensions.Logging;
using Elevator.Services;

namespace Elevator
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

            builder.Services.AddMauiBlazorWebView();

            // Register authentication services
            builder.Services.AddHttpClient<AuthApiClient>(client =>
            {
                // Configure base address - this should be updated to match your API URL
                client.BaseAddress = new Uri("https://localhost:7001/"); // Default development URL
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddSingleton<SecureTokenStorage>();
            builder.Services.AddSingleton<AuthStateService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
