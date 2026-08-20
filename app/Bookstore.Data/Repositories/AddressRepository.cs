using Bookstore.Domain.Addresses;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AddressRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        async Task IAddressRepository.DeleteAsync(string sub, int id)
        {
            var address = await _dbContext.Address.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id);

            if (address == null) return;

            address.IsActive = false;
        }

        async Task<Address?> IAddressRepository.GetAsync(string sub, int id)
        {
            return await _dbContext.Address.SingleOrDefaultAsync(x => x.Customer.Sub == sub && x.Id == id && x.IsActive);
        }

        async Task<IEnumerable<Address>> IAddressRepository.ListAsync(string sub)
        {
            return await _dbContext.Address.Where(x => x.Customer.Sub == sub && x.IsActive).ToListAsync();
        }

        async Task IAddressRepository.AddAsync(Address address)
        {
            await _dbContext.Address.AddAsync(address);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
