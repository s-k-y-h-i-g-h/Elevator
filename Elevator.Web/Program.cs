using Elevator.Web.Components;
using Elevator.Shared.Data;
using Elevator.Shared.Models.Users;
using Elevator.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework and Database Context
builder.Services.AddDbContext<ElevatorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Elevator.Web")));

// Add ASP.NET Core Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ElevatorDbContext>();

// Add JWT Authentication for API endpoints (used by MAUI app)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-that-should-be-at-least-32-characters-long";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add API Controllers
builder.Services.AddControllers();

// Add Radzen services
builder.Services.AddScoped<NotificationService>();

// Register service implementations
builder.Services.AddScoped<IInterventionService, Elevator.Shared.Services.Implementations.WebInterventionService>();
builder.Services.AddScoped<IProtocolService, Elevator.Shared.Services.Implementations.WebProtocolService>();
builder.Services.AddScoped<IUserService, Elevator.Shared.Services.Implementations.WebUserService>();
builder.Services.AddScoped<IDiscussionService, Elevator.Shared.Services.Implementations.WebDiscussionService>();
builder.Services.AddScoped<IRatingService, Elevator.Shared.Services.Implementations.WebRatingService>();
builder.Services.AddScoped<IAuthenticationService, Elevator.Shared.Services.Implementations.WebAuthenticationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map API Controllers
app.MapControllers();

// Seed database in development (but not in testing)
if (app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
            await Elevator.Web.TestSeeding.TestDatabaseConnectionAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }
