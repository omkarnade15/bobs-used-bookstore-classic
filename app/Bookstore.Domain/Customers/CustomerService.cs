namespace Bookstore.Domain.Customers
{
    public interface ICustomerService
    {
        Task<Customer?> GetAsync(int id);

        Task<Customer?> GetAsync(string sub);

        Task CreateOrUpdateCustomerAsync(CreateOrUpdateCustomerDto createOrUpdateCustomerDto);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer?> GetAsync(int id)
        {
            return await _customerRepository.GetAsync(id);
        }

        public async Task<Customer?> GetAsync(string sub)
        {
            return await _customerRepository.GetAsync(sub);
        }

        public async Task CreateOrUpdateCustomerAsync(CreateOrUpdateCustomerDto dto)
        {
            var existingCustomer = await _customerRepository.GetAsync(dto.CustomerSub);

            if (existingCustomer == null)
            {
                existingCustomer = new Customer();
                await _customerRepository.AddAsync(existingCustomer);
            }

            existingCustomer.Sub = dto.CustomerSub;
            existingCustomer.Username = dto.Username;
            existingCustomer.FirstName = dto.FirstName;
            existingCustomer.LastName = dto.LastName;
            existingCustomer.UpdatedOn = DateTime.UtcNow;

            await _customerRepository.SaveChangesAsync();
        }
    }
}
