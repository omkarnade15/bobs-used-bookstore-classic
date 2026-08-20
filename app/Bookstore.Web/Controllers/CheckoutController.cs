using Bookstore.Domain.Addresses;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Orders;
using Bookstore.Web.Helpers;
using Bookstore.Web.ViewModel.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderService _orderService;

        public CheckoutController(IShoppingCartService shoppingCartService,
                                  IOrderService orderService,
                                  IAddressService addressService)
        {
            _shoppingCartService = shoppingCartService;
            _orderService = orderService;
            _addressService = addressService;
        }

        public async Task<IActionResult> Index()
        {
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(HttpContext.GetShoppingCartCorrelationId());
            var addresses = await _addressService.GetAddressesAsync(User.GetSub()!);
            return View(new CheckoutIndexViewModel(shoppingCart, addresses));
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutIndexViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var dto = new CreateOrderDto(User.GetSub()!, HttpContext.GetShoppingCartCorrelationId(), model.SelectedAddressId);
            var orderId = await _orderService.CreateOrderAsync(dto);
            return RedirectToAction("Finished", new { orderId });
        }

        public async Task<IActionResult> Finished(int orderId)
        {
            var order = await _orderService.GetOrderAsync(orderId);
            return View(new CheckoutFinishedViewModel(order));
        }
    }
}
