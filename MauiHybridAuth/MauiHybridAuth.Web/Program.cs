using MauiHybridAuth.Shared.Services;
using MauiHybridAuth.Web.Components;
using MauiHybridAuth.Web.Components.Account;
using MauiHybridAuth.Web.Data;
using MauiHybridAuth.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Anthropic.SDK;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the MauiHybridAuth.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<ICompoundService, CompoundService>();
builder.Services.AddScoped<IInterventionService, InterventionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Add Claude AI service
builder.Services.AddSingleton<AnthropicClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["Claude:ApiKey"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
    
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("Claude API key not found. Please set 'Claude:ApiKey' in configuration or 'ANTHROPIC_API_KEY' environment variable.");
    }
    
    return new AnthropicClient(apiKey);
});
builder.Services.AddScoped<ClaudeService>();

// Add Auth services used by the Web app
builder.Services.AddAuthentication(options =>
{
    // Ensure that unauthenticated clients redirect to the login page rather than receive a 401 by default.
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => 
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
        .UseSeeding((context, _) =>
        {
            // Synchronous seeding for migrations
            DatabaseSeeder.SeedAllSync((ApplicationDbContext)context);
        })
        .UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            // Async seeding for application startup and admin interface
            await DatabaseSeeder.SeedAllAsync((ApplicationDbContext)context, cancellationToken);
        }));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Needed for external clients to log in
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(MauiHybridAuth.Shared._Imports).Assembly);

// Needed for external clients to log in
app.MapGroup("/identity").MapIdentityApi<ApplicationUser>();
// Needed for Identity Blazor components
app.MapAdditionalIdentityEndpoints();

app.Run();