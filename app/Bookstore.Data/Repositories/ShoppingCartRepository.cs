using Bookstore.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ShoppingCartRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        async Task IShoppingCartRepository.AddAsync(ShoppingCart shoppingCart)
        {
            await _dbContext.ShoppingCart.AddAsync(shoppingCart);
        }

        async Task<ShoppingCart?> IShoppingCartRepository.GetAsync(string correlationId)
        {
            return await _dbContext.ShoppingCart
                .Include(x => x.ShoppingCartItems)
                    .ThenInclude(y => y.Book)
                .SingleOrDefaultAsync(x => x.CorrelationId == correlationId);
        }

        async Task IShoppingCartRepository.SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
