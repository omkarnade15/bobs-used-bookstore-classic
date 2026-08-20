using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Addresses
{
    public interface IAddressService
    {
        Task<Address?> GetAddressAsync(string sub, int id);

        Task<IEnumerable<Address>> GetAddressesAsync(string sub);

        Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto);

        Task CreateAddressAsync(CreateAddressDto createAddressDto);

        Task UpdateAddressAsync(UpdateAddressDto updateAddressDto);
    }

    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICustomerRepository _customerRepository;

        public AddressService(IAddressRepository addressRepository, ICustomerRepository customerRepository)
        {
            _addressRepository = addressRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Address?> GetAddressAsync(string sub, int id)
        {
            return await _addressRepository.GetAsync(sub, id);
        }

        public async Task<IEnumerable<Address>> GetAddressesAsync(string sub)
        {
            return await _addressRepository.ListAsync(sub);
        }

        public async Task CreateAddressAsync(CreateAddressDto dto)
        {
            var customer = await _customerRepository.GetAsync(dto.CustomerSub)
                ?? throw new InvalidOperationException($"Customer with sub '{dto.CustomerSub}' not found.");

            var address = new Address(customer, dto.AddressLine1, dto.AddressLine2, dto.City, dto.State, dto.Country, dto.ZipCode);

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveChangesAsync();
        }

        public async Task UpdateAddressAsync(UpdateAddressDto dto)
        {
            var address = await _addressRepository.GetAsync(dto.CustomerSub, dto.AddressId);
            if (address == null) return;

            address.AddressLine1 = dto.AddressLine1;
            address.AddressLine2 = dto.AddressLine2;
            address.City = dto.City;
            address.State = dto.State;
            address.Country = dto.Country;
            address.ZipCode = dto.ZipCode;

            await _addressRepository.SaveChangesAsync();
        }

        public async Task DeleteAddressAsync(DeleteAddressDto dto)
        {
            await _addressRepository.DeleteAsync(dto.CustomerSub, dto.AddressId);
            await _addressRepository.SaveChangesAsync();
        }
    }
}
