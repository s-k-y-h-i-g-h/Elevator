using Microsoft.Extensions.Logging;
using Elevator.Maui.Services;
using Elevator.Shared.Services.Interfaces;

namespace Elevator.Maui;

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

		// Configure HTTP client with base address
		builder.Services.AddHttpClient("ElevatorAPI", client =>
		{
			// TODO: Configure this from appsettings or environment
			client.BaseAddress = new Uri("https://localhost:7001/api/");
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Register HTTP client services
		builder.Services.AddScoped<IAuthenticationService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ElevatorAPI");
			var secureStorage = provider.GetRequiredService<ISecureStorage>();
			return new HttpAuthenticationService(httpClient, secureStorage);
		});

		builder.Services.AddScoped<IInterventionService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ElevatorAPI");
			var authService = provider.GetRequiredService<IAuthenticationService>();
			return new HttpInterventionService(httpClient, authService);
		});

		builder.Services.AddScoped<IProtocolService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ElevatorAPI");
			var authService = provider.GetRequiredService<IAuthenticationService>();
			return new HttpProtocolService(httpClient, authService);
		});

		builder.Services.AddScoped<IUserService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ElevatorAPI");
			var authService = provider.GetRequiredService<IAuthenticationService>();
			return new HttpUserService(httpClient, authService);
		});

		builder.Services.AddScoped<IDiscussionService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ElevatorAPI");
			var authService = provider.GetRequiredService<IAuthenticationService>();
			return new HttpDiscussionService(httpClient, authService);
		});

		builder.Services.AddScoped<IRatingService>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient("ElevatorAPI");
			var authService = provider.GetRequiredService<IAuthenticationService>();
			return new HttpRatingService(httpClient, authService);
		});

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
