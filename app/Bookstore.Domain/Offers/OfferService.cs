using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Offers
{
    public interface IOfferService
    {
        Task<IPaginatedList<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize);

        Task<IEnumerable<Offer>> GetOffersAsync(string sub);

        Task<Offer?> GetOfferAsync(int offerId);

        Task CreateOfferAsync(CreateOfferDto createOfferDto);

        Task UpdateOfferStatusAsync(UpdateOfferStatusDto updateOfferStatusDto);

        Task<OfferStatistics> GetStatisticsAsync();
    }

    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly ICustomerRepository _customerRepository;

        public OfferService(IOfferRepository offerRepository, ICustomerRepository customerRepository)
        {
            _offerRepository = offerRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IPaginatedList<Offer>> GetOffersAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            return await _offerRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Offer>> GetOffersAsync(string sub)
        {
            return await _offerRepository.ListAsync(sub);
        }

        public async Task<Offer?> GetOfferAsync(int id)
        {
            return await _offerRepository.GetAsync(id);
        }

        public async Task CreateOfferAsync(CreateOfferDto dto)
        {
            var customer = await _customerRepository.GetAsync(dto.CustomerSub)
                ?? throw new InvalidOperationException($"Customer with sub '{dto.CustomerSub}' not found.");

            var offer = new Offer(
                customer.Id,
                dto.BookName,
                dto.Author,
                dto.ISBN,
                dto.BookTypeId,
                dto.ConditionId,
                dto.GenreId,
                dto.PublisherId,
                dto.BookPrice);

            await _offerRepository.AddAsync(offer);
            await _offerRepository.SaveChangesAsync();
        }

        public async Task UpdateOfferStatusAsync(UpdateOfferStatusDto dto)
        {
            var offer = await GetOfferAsync(dto.OfferId);
            if (offer == null) return;

            offer.OfferStatus = dto.Status;
            offer.UpdatedOn = DateTime.UtcNow;

            await _offerRepository.SaveChangesAsync();
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            return await _offerRepository.GetStatisticsAsync();
        }
    }
}
