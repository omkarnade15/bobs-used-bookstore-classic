using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Web.Helpers;
using Bookstore.Web.ViewModel.ShoppingCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    [AllowAnonymous]
    public class ShoppingCartController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(ICustomerService customerService, IShoppingCartService shoppingCartService)
        {
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index()
        {
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(HttpContext.GetShoppingCartCorrelationId());
            return View(new ShoppingCartIndexViewModel(shoppingCart));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int shoppingCartItemId)
        {
            var dto = new DeleteShoppingCartItemDto(HttpContext.GetShoppingCartCorrelationId(), shoppingCartItemId);
            await _shoppingCartService.DeleteShoppingCartItemAsync(dto);
            this.SetNotification("Item removed from shopping cart.");
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
