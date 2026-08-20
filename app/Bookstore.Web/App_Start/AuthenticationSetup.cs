using BobsBookstoreClassic.Data;
using Bookstore.Domain.Customers;
using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Bookstore.Web
{
    public static class AuthenticationConfig
    {
        public static void ConfigureAuthentication(IServiceCollection services)
        {
            if (BookstoreConfiguration.GetSetting("Services/Authentication") == "aws")
            {
                ConfigureCognitoAuthentication(services);
            }
            // Local authentication is handled by LocalAuthenticationMiddleware
        }

        private static void ConfigureCognitoAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.ClientId = BookstoreConfiguration.GetSetting("Authentication/Cognito/LocalClientId");
                options.MetadataAddress = BookstoreConfiguration.GetSetting("Authentication/Cognito/MetadataAddress");
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
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
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.RedirectUri = context.Request.GetReturnUrl();
                        return Task.CompletedTask;
                    },
                    OnAuthorizationCodeReceived = context =>
                    {
                        context.TokenEndpointRequest!.RedirectUri = context.Request.GetReturnUrl();
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var service = context.HttpContext.RequestServices.GetRequiredService<ICustomerService>();
                        var identity = (ClaimsIdentity?)context.Principal?.Identity;
                        if (identity == null) return;

                        var dto = new CreateOrUpdateCustomerDto(
                            identity.GetSub() ?? string.Empty,
                            identity.Name ?? string.Empty,
                            identity.FindFirst(c => c.Type.Contains("givenname"))?.Value ?? string.Empty,
                            identity.FindFirst(c => c.Type.Contains("surname"))?.Value ?? string.Empty);

                        await service.CreateOrUpdateCustomerAsync(dto);
                    }
                };
            });
        }
    }
}
