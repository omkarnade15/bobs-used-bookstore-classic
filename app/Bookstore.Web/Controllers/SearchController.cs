using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Web.Helpers;
using Bookstore.Web.ViewModel.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    [AllowAnonymous]
    public class SearchController : Controller
    {
        private readonly IBookService _inventoryService;
        private readonly IShoppingCartService _shoppingCartService;

        public SearchController(IBookService inventoryService, IShoppingCartService shoppingCartService)
        {
            _inventoryService = inventoryService;
            _shoppingCartService = shoppingCartService;
        }

        public async Task<IActionResult> Index(string? searchString, string sortBy = "Name", int pageIndex = 1, int pageSize = 10)
        {
            var books = await _inventoryService.GetBooksAsync(searchString ?? string.Empty, sortBy, pageIndex, pageSize);
            return View(new SearchIndexViewModel(books));
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _inventoryService.GetBookAsync(id);
            return View(new SearchDetailsViewModel(book));
        }

        public async Task<IActionResult> AddItemToShoppingCart(int bookId)
        {
            var dto = new AddToShoppingCartDto(HttpContext.GetShoppingCartCorrelationId(), bookId, 1);
            await _shoppingCartService.AddToShoppingCartAsync(dto);
            this.SetNotification("Item added to shopping cart");
            return RedirectToAction("Index", "Search");
        }

        public async Task<IActionResult> AddItemToWishlist(int bookId)
        {
            var dto = new AddToWishlistDto(HttpContext.GetShoppingCartCorrelationId(), bookId);
            await _shoppingCartService.AddToWishlistAsync(dto);
            this.SetNotification("Item added to wishlist");
            return RedirectToAction("Index", "Search");
        }
    }
}
