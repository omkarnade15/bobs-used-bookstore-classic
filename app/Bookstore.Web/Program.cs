using Amazon.Rekognition;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using BobsBookstoreClassic.Data;
using Bookstore.Common;
using Bookstore.Data;
using Bookstore.Data.FileServices;
using Bookstore.Data.ImageResizeService;
using Bookstore.Data.ImageValidationServices;
using Bookstore.Data.Repositories;
using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// ─── Configuration ────────────────────────────────────────────────────────────
// Seed the static BookstoreConfiguration from IConfiguration for legacy code paths.
BookstoreConfiguration.Initialize(builder.Configuration);

// Load AppSettings keys into BookstoreConfiguration under the flat key format
// (e.g., "Services/Authentication") expected by the rest of the code.
foreach (var item in builder.Configuration.GetSection("AppSettings").GetChildren())
{
    BookstoreConfiguration.AddSetting(item.Key, item.Value ?? string.Empty);
}

// Load connection strings
foreach (var cs in builder.Configuration.GetSection("ConnectionStrings").GetChildren())
{
    BookstoreConfiguration.AddConnectionString(cs.Key, cs.Value ?? string.Empty);
}

// If using AWS Parameter Store for configuration, load those settings now
LoadAwsConfiguration(builder.Configuration);

// ─── Logging ──────────────────────────────────────────────────────────────────
ConfigureNLog();
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// ─── EF Core ──────────────────────────────────────────────────────────────────
var connectionString = BookstoreConfiguration.GetConnectionString("BookstoreDatabaseConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ─── Application Services (DI) ────────────────────────────────────────────────
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReferenceDataService, ReferenceDataService>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IImageResizeService, ImageResizeService>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();

builder.Services.AddScoped(typeof(IPaginatedList<>), typeof(PaginatedList<>));

// File service
if (BookstoreConfiguration.GetSetting("Services/FileService") == "aws")
{
    builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>();
    builder.Services.AddScoped<IFileService, S3FileService>();
}
else
{
    builder.Services.AddSingleton<IFileService>(sp =>
    {
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var contentPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "Content");
        return new LocalFileService(contentPath);
    });
}

// Image validation service
if (BookstoreConfiguration.GetSetting("Services/ImageValidationService") == "aws")
{
    builder.Services.AddSingleton<IAmazonRekognition, AmazonRekognitionClient>();
    builder.Services.AddScoped<IImageValidationService, RekognitionImageValidationService>();
}
else
{
    builder.Services.AddScoped<IImageValidationService, LocalImageValidationService>();
}

// Local auth middleware (only needed when not using Cognito)
if (BookstoreConfiguration.GetSetting("Services/Authentication") != "aws")
{
    builder.Services.AddScoped<LocalAuthenticationMiddleware>();
}

// ─── Authentication ────────────────────────────────────────────────────────────
if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
{
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = BookstoreConfiguration.GetSetting("Authentication/Cognito/LocalClientId");
            options.MetadataAddress = BookstoreConfiguration.GetSetting("Authentication/Cognito/MetadataAddress");
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.SaveTokens = true;
            options.UseTokenLifetime = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "cognito:username",
                RoleClaimType = "cognito:groups"
            };
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = ctx =>
                {
                    ctx.ProtocolMessage.RedirectUri = GetReturnUrl(ctx.Request);
                    return Task.CompletedTask;
                },
                OnAuthorizationCodeReceived = ctx =>
                {
                    ctx.TokenEndpointRequest!.RedirectUri = GetReturnUrl(ctx.Request);
                    return Task.CompletedTask;
                },
                OnTokenValidated = async ctx =>
                {
                    var service = ctx.HttpContext.RequestServices.GetRequiredService<ICustomerService>();
                    var identity = ctx.Principal?.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        var dto = new CreateOrUpdateCustomerDto(
                            identity.FindFirst(c => c.Type.Contains("nameidentifier"))?.Value ?? string.Empty,
                            identity.Name ?? string.Empty,
                            identity.FindFirst(c => c.Type.Contains("givenname"))?.Value ?? string.Empty,
                            identity.FindFirst(c => c.Type.Contains("surname"))?.Value ?? string.Empty);
                        await service.CreateOrUpdateCustomerAsync(dto);
                    }
                }
            };
        });
}
else
{
    builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/Authentication/Login";
        });
}

// ─── MVC ──────────────────────────────────────────────────────────────────────
builder.Services
    .AddControllersWithViews(options =>
    {
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    })
    .AddRazorRuntimeCompilation();

builder.Services.AddHttpContextAccessor();

// ─── Build App ────────────────────────────────────────────────────────────────
var app = builder.Build();

// Ensure DB is created and seeded during development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.EnsureCreated();
        BookstoreDbInitializer.Seed(db);
    }
    catch (Exception ex)
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Error(ex, "Failed to initialize database");
    }
}

// ─── Middleware Pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

// Local auth middleware (injects claims for dev/test without Cognito)
if (BookstoreConfiguration.GetSetting("Services/Authentication") != "aws")
{
    app.UseMiddleware<LocalAuthenticationMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();

// ─── Routing ──────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "Admin_default",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" },
    constraints: new { },
    dataTokens: new { area = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// ─── Helpers ──────────────────────────────────────────────────────────────────
static string GetReturnUrl(HttpRequest request)
{
    return $"{request.Scheme}://{request.Host}/signin-oidc";
}

static void ConfigureNLog()
{
    var config = new LoggingConfiguration();
    NLog.Targets.Target loggingTarget;

    if (BookstoreConfiguration.GetSetting("Services/LoggingService") == "aws")
    {
        loggingTarget = new AWSTarget { LogGroup = Constants.AppName };
    }
    else
    {
        loggingTarget = new DebuggerTarget("debugger");
    }

    config.AddTarget(loggingTarget);
    config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, loggingTarget));
    LogManager.Configuration = config;
}

static void LoadAwsConfiguration(IConfiguration configuration)
{
    var rootPath = "/" + Constants.AppName;

    if (BookstoreConfiguration.GetSetting("Services/Database") == "aws")
    {
        try
        {
            using var client = new AmazonSimpleSystemsManagementClient();
            var request = new GetParameterRequest
            {
                Name = $"{rootPath}/Database/ConnectionStrings/BookstoreDatabaseConnection"
            };
            var response = client.GetParameterAsync(request).GetAwaiter().GetResult();
            BookstoreConfiguration.AddConnectionString(
                "BookstoreDatabaseConnection",
                response.Parameter.Value);
        }
        catch { /* non-blocking: will fall back to local config */ }
    }

    if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
    {
        try
        {
            using var client = new AmazonSimpleSystemsManagementClient();
            var request = new GetParametersByPathRequest
            {
                Path = $"{rootPath}/Authentication/",
                Recursive = true
            };
            var response = client.GetParametersByPathAsync(request).GetAwaiter().GetResult();
            foreach (var parameter in response.Parameters)
            {
                BookstoreConfiguration.AddSetting(
                    parameter.Name.Replace($"{rootPath}/", string.Empty),
                    parameter.Value);
            }
        }
        catch { /* non-blocking */ }
    }

    if (BookstoreConfiguration.GetSetting("Services/FileService") == "aws")
    {
        try
        {
            using var client = new AmazonSimpleSystemsManagementClient();
            var request = new GetParametersByPathRequest
            {
                Path = $"{rootPath}/Files/",
                Recursive = true
            };
            var response = client.GetParametersByPathAsync(request).GetAwaiter().GetResult();
            foreach (var parameter in response.Parameters)
            {
                BookstoreConfiguration.AddSetting(
                    parameter.Name.Replace($"{rootPath}/", string.Empty),
                    parameter.Value);
            }
        }
        catch { /* non-blocking */ }
    }
}
