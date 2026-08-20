using Microsoft.AspNetCore.Http;

namespace Bookstore.Web.Helpers
{
    public static class HttpContextExtensions
    {
        private const string CookieKey = "ShoppingCartId";

        public static string GetShoppingCartCorrelationId(this HttpContext context)
        {
            string? shoppingCartClientId = context.Request.Cookies[CookieKey];

            if (string.IsNullOrWhiteSpace(shoppingCartClientId))
            {
                shoppingCartClientId = context.User.Identity?.IsAuthenticated == true
                    ? context.User.GetSub()
                    : Guid.NewGuid().ToString();
            }

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                Path = "/"
            };

            context.Response.Cookies.Append(CookieKey, shoppingCartClientId!, cookieOptions);

            return shoppingCartClientId!;
        }
    }
}
