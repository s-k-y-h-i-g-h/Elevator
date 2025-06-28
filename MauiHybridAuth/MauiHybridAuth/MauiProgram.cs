using MauiHybridAuth.Services;
using MauiHybridAuth.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Radzen;

namespace MauiHybridAuth
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

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Add Radzen services
            builder.Services.AddRadzenComponents();

            //Register needed elements for authentication:
            // This is the core functionality
            builder.Services.AddAuthorizationCore();
            // This is our custom provider
            builder.Services.AddScoped<MauiAuthenticationStateProvider, MauiAuthenticationStateProvider>();
            // Use our custom provider when the app needs an AuthenticationStateProvider
            builder.Services.AddScoped<AuthenticationStateProvider>(s
                => (MauiAuthenticationStateProvider)s.GetRequiredService<MauiAuthenticationStateProvider>());

            // Add device-specific services used by the MauiHybridAuth.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddScoped<IWeatherService, WeatherService>();

            return builder.Build();
        }
    }
}
