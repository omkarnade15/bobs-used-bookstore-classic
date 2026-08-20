using Bookstore.Domain.Customers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Bookstore.Web.Helpers
{
    public class LocalAuthenticationMiddleware
    {
        private const string UserId = "FB6135C7-1464-4A72-B74E-4B63D343DD09";
        private const string LocalAuthCookie = "LocalAuthentication";

        private readonly RequestDelegate _next;

        public LocalAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICustomerService customerService)
        {
            if (context.Request.Path.StartsWithSegments("/Authentication/Login"))
            {
                SetClaimsPrincipal(context);

                await SaveCustomerDetailsAsync(context, customerService);

                context.Response.Cookies.Append(LocalAuthCookie, "1", new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(1)
                });

                context.Response.Redirect("/");
                return;
            }

            if (context.Request.Cookies.ContainsKey(LocalAuthCookie))
            {
                SetClaimsPrincipal(context);

                await SaveCustomerDetailsAsync(context, customerService);
            }

            await _next(context);
        }

        private static void SetClaimsPrincipal(HttpContext context)
        {
            var identity = new ClaimsIdentity("Application");

            identity.AddClaim(new Claim(ClaimTypes.Name, "bookstoreuser"));
            identity.AddClaim(new Claim("nameidentifier", UserId));
            identity.AddClaim(new Claim("given_name", "Bookstore"));
            identity.AddClaim(new Claim("family_name", "User"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrators"));

            context.User = new ClaimsPrincipal(identity);
        }

        private static async Task SaveCustomerDetailsAsync(HttpContext context, ICustomerService customerService)
        {
            var identity = (ClaimsIdentity?)context.User.Identity;
            if (identity == null) return;

            var dto = new CreateOrUpdateCustomerDto(
                identity.FindFirst("nameidentifier")?.Value ?? string.Empty,
                identity.Name ?? string.Empty,
                identity.FindFirst("given_name")?.Value ?? string.Empty,
                identity.FindFirst("family_name")?.Value ?? string.Empty);

            await customerService.CreateOrUpdateCustomerAsync(dto);
        }
    }
}
