using Bookstore.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomerRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        async Task ICustomerRepository.AddAsync(Customer customer)
        {
            await _dbContext.Customer.AddAsync(customer);
        }

        async Task<Customer?> ICustomerRepository.GetAsync(int id)
        {
            return await _dbContext.Customer.FindAsync(id);
        }

        async Task<Customer?> ICustomerRepository.GetAsync(string sub)
        {
            return await _dbContext.Customer.SingleOrDefaultAsync(x => x.Sub == sub);
        }

        async Task ICustomerRepository.SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
