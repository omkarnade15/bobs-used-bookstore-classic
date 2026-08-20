using BobsBookstoreClassic.Data;
using Bookstore.Data;
using Bookstore.Web;
using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Initialize static configuration accessor from IConfiguration
BookstoreConfiguration.Initialize(builder.Configuration);

// Load additional settings from AWS SSM (database connection string, Cognito settings, etc.)
await ConfigurationSetup.ConfigureConfigurationAsync(builder.Configuration);

// Configure NLog
LoggingSetup.ConfigureLogging();

// MVC + Views
builder.Services.AddControllersWithViews(options =>
{
    // Require authenticated users by default; controllers/actions use [AllowAnonymous] to opt out
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});

// HTTP context accessor (required for middleware helpers)
builder.Services.AddHttpContextAccessor();

// Domain services, repositories, EF Core DbContext
DependencyInjectionSetup.ConfigureDependencyInjection(builder.Services, builder.Environment);

// Authentication (local or Cognito)
AuthenticationConfig.ConfigureAuthentication(builder.Services);

// For local auth scenario we still need a minimal cookie scheme so [Authorize] works
if (BookstoreConfiguration.GetSetting("Services/Authentication") != "aws")
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();
}

var app = builder.Build();

// Database seeding
await BookstoreDbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Wire up local authentication middleware before Authorization
if (BookstoreConfiguration.GetSetting("Services/Authentication") != "aws")
{
    app.UseMiddleware<LocalAuthenticationMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

// Admin area route
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
