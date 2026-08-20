using Bookstore.Domain.Books;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Web.Areas.Admin.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Areas.Admin.Controllers
{
    public class DashboardController : AdminAreaControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOfferService _offerService;
        private readonly IBookService _bookService;

        public DashboardController(IOrderService orderService, IOfferService offerService, IBookService bookService)
        {
            _orderService = orderService;
            _offerService = offerService;
            _bookService = bookService;
        }

        public async Task<IActionResult> Index()
        {
            var orderStats = await _orderService.GetStatisticsAsync();
            var offerStats = await _offerService.GetStatisticsAsync();
            var inventoryStats = await _bookService.GetStatisticsAsync();

            var model = new DashboardIndexViewModel
            {
                PastDueOrders = orderStats.PastDueOrders,
                PendingOrders = orderStats.PendingOrders,
                OrdersThisMonth = orderStats.OrdersThisMonth,
                OrdersTotal = orderStats.OrdersTotal,

                PendingOffers = offerStats.PendingOffers,
                OffersThisMonth = offerStats.OffersThisMonth,
                OffersTotal = offerStats.OffersTotal,

                LowStock = inventoryStats.LowStock,
                OutOfStock = inventoryStats.OutOfStock,
                StockTotal = inventoryStats.StockTotal
            };

            return View(model);
        }
    }
}
