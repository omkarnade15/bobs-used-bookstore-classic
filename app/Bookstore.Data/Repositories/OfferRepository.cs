using Bookstore.Domain;
using Bookstore.Domain.Offers;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OfferRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OfferStatistics> GetStatisticsAsync()
        {
            var startOfMonth = DateTime.UtcNow.StartOfMonth();

            return await _dbContext.Offer
                .GroupBy(x => 1)
                .Select(x => new OfferStatistics
                {
                    PendingOffers = x.Count(y => y.OfferStatus == OfferStatus.PendingApproval),
                    OffersThisMonth = x.Count(y => y.CreatedOn >= startOfMonth),
                    OffersTotal = x.Count()
                })
                .SingleOrDefaultAsync() ?? new OfferStatistics();
        }

        async Task IOfferRepository.AddAsync(Offer offer)
        {
            await _dbContext.Offer.AddAsync(offer);
        }

        Task<Offer?> IOfferRepository.GetAsync(int id)
        {
            return _dbContext.Offer.Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id);
        }

        async Task<IPaginatedList<Offer>> IOfferRepository.ListAsync(OfferFilters filters, int pageIndex, int pageSize)
        {
            var query = _dbContext.Offer.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Author))
                query = query.Where(x => x.Author.Contains(filters.Author));

            if (!string.IsNullOrWhiteSpace(filters.BookName))
                query = query.Where(x => x.BookName.Contains(filters.BookName));

            if (filters.ConditionId.HasValue)
                query = query.Where(x => x.ConditionId == filters.ConditionId);

            if (filters.GenreId.HasValue)
                query = query.Where(x => x.GenreId == filters.GenreId);

            if (filters.OfferStatus.HasValue)
                query = query.Where(x => x.OfferStatus == filters.OfferStatus);

            query = query
                .Include(x => x.Customer)
                .Include(x => x.Condition)
                .Include(x => x.Genre);

            var result = new PaginatedList<Offer>(query, pageIndex, pageSize);
            await result.PopulateAsync();
            return result;
        }

        async Task<IEnumerable<Offer>> IOfferRepository.ListAsync(string sub)
        {
            return await _dbContext.Offer
                .Include(x => x.BookType)
                .Include(x => x.Genre)
                .Include(x => x.Condition)
                .Include(x => x.Publisher)
                .Where(x => x.Customer.Sub == sub)
                .ToListAsync();
        }

        async Task IOfferRepository.SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
