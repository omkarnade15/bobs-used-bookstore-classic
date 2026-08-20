# Migration Summary: .NET Framework 4.8 → .NET 10

## Status
**Build result: SUCCEEDED — 0 errors, 0 code warnings**

All NU19xx warnings are NuGet vulnerability advisories for Magick.NET 14.7.0 (image processing library). See "Next Steps" for upgrade path.

---

## What was migrated

### Project files
| Project | From | To |
|---|---|---|
| Bookstore.Web | Old-style .csproj (net4.8, System.Web.Mvc) | SDK-style `Microsoft.NET.Sdk.Web` targeting `net10.0` |
| Bookstore.Data | `netstandard2.0` + EF6 | `net10.0` + EF Core 10 |
| Bookstore.Domain | `netstandard2.0` | `net10.0` |
| Bookstore.Common | `netstandard2.0` | `net10.0` |

### Application startup
- `Global.asax` + `Startup.cs` (OWIN) → `Program.cs` (ASP.NET Core minimal hosting)
- `App_Start/RouteConfig.cs` → `app.MapAreaControllerRoute` + `app.MapControllerRoute` in Program.cs
- `App_Start/BundleConfig.cs` → removed (static files served from wwwroot directly)
- `App_Start/FilterConfig.cs` → `options.Filters.Add(new AuthorizeFilter())` in `AddControllersWithViews`

### Authentication
- OWIN `Microsoft.Owin.Security.OpenIdConnect` → `Microsoft.AspNetCore.Authentication.OpenIdConnect`
- OWIN `CookieAuthentication` → `AddCookie()` in ASP.NET Core
- `LocalAuthenticationMiddleware` rewritten as `IMiddleware`-compatible ASP.NET Core middleware
- `IOwinRequestExtensions` (`IOwinRequest`) → `OwinRequestExtensions` (`HttpRequest`)

### Configuration
- `Web.config` `<appSettings>` → `appsettings.json`
- `System.Configuration.ConfigurationManager` → `IConfiguration` via `BookstoreConfiguration.Initialize(IConfiguration)`
- AWS SSM parameter loading preserved in `ConfigurationSetup.ConfigureConfigurationAsync`

### Data access (EF6 → EF Core 10)
- `ApplicationDbContext` constructor changed from `(string connectionString)` to `(DbContextOptions<ApplicationDbContext>)`
- EF6 `DbModelBuilder` API → EF Core `ModelBuilder` API
  - `HasRequired().WithMany().WillCascadeOnDelete(false)` → `HasOne().WithMany().OnDelete(DeleteBehavior.Restrict)`
  - `HasDatabaseGeneratedOption(Identity)` → `ValueGeneratedOnAdd()`
  - `PluralizingTableNameConvention` removed → explicit `ToTable()` calls
- `System.Data.Entity` → `Microsoft.EntityFrameworkCore` in all repositories
- EF6 `Include("string")` patterns → strongly-typed `.Include(x => x.Nav).ThenInclude(y => y.Sub)` (EF Core)
- `BookstoreDbInitializer` (EF6 DropCreateDatabaseIfModelChanges) → `BookstoreDbSeeder` static service called at startup
- `PaginatedList<T>` updated to use EF Core `CountAsync`/`ToListAsync`

### Controllers & MVC
- `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc` in all controllers
- `ActionResult` return types → `IActionResult`
- `AreaRegistration` (Areas/Admin/AdminAreaRegistration.cs) → `[Area("Admin")]` attribute on `AdminAreaControllerBase`
- HTTP file upload: `HttpPostedFileBase` → `IFormFile`; `.InputStream` → `.OpenReadStream()`

### View models & helpers
- `System.Web.Mvc.SelectListItem` → `Microsoft.AspNetCore.Mvc.Rendering.SelectListItem`
- `HtmlHelper` → `IHtmlHelper` in `MvcHelpers.cs`
- `HttpContextBase` → `HttpContext` in `HttpContextExtensions.cs`
- `HttpPostedFileBase` → `IFormFile` in `ImageTypesAttribute`, `MaxFileSizeAttribute`, `InventoryCreateUpdateViewModel`
- Nullable reference types applied throughout

### Views
- `Views/Web.config` and `Areas/Admin/Views/web.config` → not used in ASP.NET Core
- `@Html.Partial(...)` → `<partial name="..." />` tag helpers (eliminates MVC1000 deadlock warnings)
- `@Html.EnumDropDownListFor(...)` → `@Html.DropDownListFor(..., Html.GetEnumSelectList<T>()...)` 
- `_ViewImports.cshtml` updated with `@using Microsoft.AspNetCore.Mvc.Rendering`
- Areas `_ViewImports.cshtml` created

### Dependency injection
- Autofac.Mvc5 + Autofac.Owin → built-in `IServiceCollection` DI
- All services and repositories registered as `AddScoped` in `DependencyInjectionSetup`

### Logging
- `NLog.AWS.Logger` (net45 build) → `AWS.Logger.NLog` 3.3.4
- Ambiguous `LogLevel` reference resolved to `NLog.LogLevel`

### Domain services
- Updated nullable return types throughout (interfaces, services, repositories)
- `OfferStatistics` namespace corrected (`Bookstore.Domain.Offers`, was incorrectly in `Bookstore.Domain.Orders`)
- `BookService`, `CustomerService`, `AddressService`, `OrderService`, `OfferService`, `ShoppingCartService`, `ReferenceDataService` all updated for nullable correctness

---

## Next steps

1. **Magick.NET vulnerabilities (NU19xx)**: Upgrade to the latest stable release (e.g. 15.x) to resolve the 640+ vulnerability advisories. Upgrade is safe once breaking API changes in the new version are reviewed; the only usage is in `ImageResizeService.ResizeImageAsync`.

2. **EF Core migrations**: The app uses `EnsureCreated()` (via `BookstoreDbSeeder`) which is fine for development but not recommended for production. Before deploying to production, create an EF Core initial migration: `dotnet ef migrations add InitialCreate --project app/Bookstore.Data --startup-project app/Bookstore.Web`. Then use `dotnet ef database update` instead of `EnsureCreated`.

3. **HTTPS for Cognito redirect URIs**: The AWS Cognito Hosted UI requires HTTPS redirect URIs in production. The `GetReturnUrl` extension method returns `http://` for local traffic. In production (ECS), configure HTTPS and update the CDK stack's load balancer to terminate TLS.

4. **CDK stack (`Bookstore.Cdk`)**: The CDK stack's Dockerfile and ECS task definitions still reference Windows containers. After this migration, the app targets `net10.0` (Linux-compatible). Update the CDK stack to use a Linux-based `mcr.microsoft.com/dotnet/aspnet:10.0` image. The existing `Dockerfile` at the repo root targets Windows — replace it with a Linux multi-stage build.

5. **Static files location**: ASP.NET Core serves static files from `wwwroot/`. The `Content/` folder (CSS, images) is referenced by existing Razor views. Either move to `wwwroot/Content/` or add `app.UseStaticFiles(new StaticFileOptions { ... })` for alternative paths. The `LocalFileService` saves cover images to `{webRootPath}/images/coverimages/` — verify this resolves correctly in production.

6. **`App_Data` folder**: Was referenced in the old `.csproj` as a folder item. No longer needed; safe to remove.

7. **Nullable warnings from Magick.NET internal API**: The image resize service calls `MagickImage(Stream)` and related APIs; the new 14.7.0 and future 15.x versions may expose nullable attributes. Review after upgrading.
