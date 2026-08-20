using Bookstore.Domain;
using Bookstore.Domain.ReferenceData;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class ReferenceDataRepository : IReferenceDataRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReferenceDataRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        async Task IReferenceDataRepository.AddAsync(ReferenceDataItem item)
        {
            await _dbContext.ReferenceData.AddAsync(item);
        }

        async Task<ReferenceDataItem?> IReferenceDataRepository.GetAsync(int id)
        {
            return await _dbContext.ReferenceData.FindAsync(id);
        }

        async Task<IEnumerable<ReferenceDataItem>> IReferenceDataRepository.FullListAsync()
        {
            return await _dbContext.ReferenceData.ToListAsync();
        }

        async Task<IPaginatedList<ReferenceDataItem>> IReferenceDataRepository.ListAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            var query = _dbContext.ReferenceData.AsQueryable();

            if (filters.ReferenceDataType.HasValue)
                query = query.Where(x => x.DataType == filters.ReferenceDataType.Value);

            var result = new PaginatedList<ReferenceDataItem>(query, pageIndex, pageSize);
            await result.PopulateAsync();
            return result;
        }

        async Task IReferenceDataRepository.SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
