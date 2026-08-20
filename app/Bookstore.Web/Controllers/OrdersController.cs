using Bookstore.Domain.Orders;
using Bookstore.Web.Helpers;
using Bookstore.Web.ViewModel.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetOrdersAsync(User.GetSub()!);
            return View(new OrderIndexViewModel(orders));
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderAsync(id);
            return View(new OrderDetailsViewModel(order));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var dto = new CancelOrderDto(User.GetSub()!, id);
            await _orderService.CancelOrderAsync(dto);
            return RedirectToAction("Index");
        }
    }
}
