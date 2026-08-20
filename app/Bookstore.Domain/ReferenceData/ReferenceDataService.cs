namespace Bookstore.Domain.ReferenceData
{
    public interface IReferenceDataService
    {
        Task<IPaginatedList<ReferenceDataItem>> GetReferenceDataAsync(ReferenceDataFilters filters, int pageIndex, int pageSize);

        Task<IEnumerable<ReferenceDataItem>> GetAllReferenceDataAsync();

        Task<ReferenceDataItem?> GetReferenceDataItemAsync(int id);

        Task CreateAsync(CreateReferenceDataItemDto createReferenceDataItemDto);

        Task UpdateAsync(UpdateReferenceDataItemDto createReferenceDataItemDto);
    }

    public class ReferenceDataService : IReferenceDataService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;

        public ReferenceDataService(IReferenceDataRepository referenceDataRepository)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        public async Task<IPaginatedList<ReferenceDataItem>> GetReferenceDataAsync(ReferenceDataFilters filters, int pageIndex, int pageSize)
        {
            return await _referenceDataRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IEnumerable<ReferenceDataItem>> GetAllReferenceDataAsync()
        {
            return await _referenceDataRepository.FullListAsync();
        }

        public async Task<ReferenceDataItem?> GetReferenceDataItemAsync(int id)
        {
            return await _referenceDataRepository.GetAsync(id);
        }

        public async Task CreateAsync(CreateReferenceDataItemDto dto)
        {
            var referenceDataItem = new ReferenceDataItem(dto.ReferenceDataType, dto.Text);
            await _referenceDataRepository.AddAsync(referenceDataItem);
            await _referenceDataRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(UpdateReferenceDataItemDto dto)
        {
            var referenceDataItem = await _referenceDataRepository.GetAsync(dto.Id);
            if (referenceDataItem == null) return;

            referenceDataItem.DataType = dto.ReferenceDataType;
            referenceDataItem.Text = dto.Text;

            await _referenceDataRepository.SaveChangesAsync();
        }
    }
}
