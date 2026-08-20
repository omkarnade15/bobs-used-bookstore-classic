namespace Bookstore.Domain.Carts
{
    public interface IShoppingCartService
    {
        Task<ShoppingCart?> GetShoppingCartAsync(string correlationId);

        Task AddToShoppingCartAsync(AddToShoppingCartDto addToShoppingCartDto);

        Task AddToWishlistAsync(AddToWishlistDto addToWishlistDto);

        Task MoveWishlistItemToShoppingCartAsync(MoveWishlistItemToShoppingCartDto moveWishlistItemToShoppingCartDto);

        Task MoveAllWishlistItemsToShoppingCartAsync(MoveAllWishlistItemsToShoppingCartDto moveAllWishlistItemsToShoppingCartDto);

        Task DeleteShoppingCartItemAsync(DeleteShoppingCartItemDto deleteShoppingCartItemDto);
    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
        }

        public async Task<ShoppingCart?> GetShoppingCartAsync(string shoppingCartCorrelationId)
        {
            return await _shoppingCartRepository.GetAsync(shoppingCartCorrelationId);
        }

        public async Task AddToShoppingCartAsync(AddToShoppingCartDto dto)
        {
            await AddToShoppingCartInternalAsync(dto.CorrelationId, dto.BookId, dto.Quantity, true);
        }

        public async Task AddToWishlistAsync(AddToWishlistDto dto)
        {
            await AddToShoppingCartInternalAsync(dto.CorrelationId, dto.BookId, 1, false);
        }

        private async Task AddToShoppingCartInternalAsync(string correlationId, int bookId, int quantity, bool wantToBuy)
        {
            var shoppingCart = await _shoppingCartRepository.GetAsync(correlationId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart(correlationId);
                await _shoppingCartRepository.AddAsync(shoppingCart);
            }

            if (wantToBuy)
                shoppingCart.AddItemToShoppingCart(bookId, quantity);
            else
                shoppingCart.AddItemToWishlist(bookId);

            await _shoppingCartRepository.SaveChangesAsync();
        }

        public async Task MoveWishlistItemToShoppingCartAsync(MoveWishlistItemToShoppingCartDto dto)
        {
            var shoppingCart = await _shoppingCartRepository.GetAsync(dto.CorrelationId);
            shoppingCart?.MoveWishListItemToShoppingCart(dto.ShoppingCartItemId);
            await _shoppingCartRepository.SaveChangesAsync();
        }

        public async Task MoveAllWishlistItemsToShoppingCartAsync(MoveAllWishlistItemsToShoppingCartDto dto)
        {
            var shoppingCart = await _shoppingCartRepository.GetAsync(dto.CorrelationId);
            if (shoppingCart == null) return;

            foreach (var wishListItem in shoppingCart.GetWishListItems())
            {
                shoppingCart.MoveWishListItemToShoppingCart(wishListItem.Id);
            }

            await _shoppingCartRepository.SaveChangesAsync();
        }

        public async Task DeleteShoppingCartItemAsync(DeleteShoppingCartItemDto dto)
        {
            var shoppingCart = await _shoppingCartRepository.GetAsync(dto.CorrelationId);
            shoppingCart?.RemoveShoppingCartItemById(dto.ShoppingCartItemId);
            await _shoppingCartRepository.SaveChangesAsync();
        }
    }
}
