using Bookstore.Domain.Orders;

namespace Bookstore.Domain.Books
{
    public interface IBookService
    {
        Task<Book> GetBookAsync(int id);

        Task<IPaginatedList<Book>> GetBooksAsync(BookFilters filters, int pageIndex, int pageSize);

        Task<IPaginatedList<Book>> GetBooksAsync(string searchString, string sortBy, int pageIndex, int pageSize);

        Task<IEnumerable<Book>> ListBestSellingBooksAsync(int count);

        Task<BookStatistics> GetStatisticsAsync();

        Task<BookResult> AddAsync(CreateBookDto createBookDto);

        Task<BookResult> UpdateAsync(UpdateBookDto updateBookDto);
    }

    public class BookService : IBookService
    {
        private readonly IImageResizeService _imageResizeService;
        private readonly IImageValidationService _imageValidationService;
        private readonly IFileService _fileService;
        private readonly IBookRepository _bookRepository;
        private readonly IOrderRepository _orderRepository;

        public BookService(
            IImageResizeService imageResizeService,
            IImageValidationService imageValidationService,
            IFileService fileService,
            IBookRepository bookRepository,
            IOrderRepository orderRepository)
        {
            _imageResizeService = imageResizeService;
            _imageValidationService = imageValidationService;
            _fileService = fileService;
            _bookRepository = bookRepository;
            _orderRepository = orderRepository;
        }

        public async Task<Book> GetBookAsync(int id)
        {
            return await _bookRepository.GetAsync(id);
        }

        public async Task<IPaginatedList<Book>> GetBooksAsync(BookFilters filters, int pageIndex, int pageSize)
        {
            return await _bookRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IPaginatedList<Book>> GetBooksAsync(string searchString, string sortBy, int pageIndex, int pageSize)
        {
            return await _bookRepository.ListAsync(searchString, sortBy, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Book>> ListBestSellingBooksAsync(int count)
        {
            return await _orderRepository.ListBestSellingBooksAsync(count);
        }

        public async Task<BookStatistics> GetStatisticsAsync()
        {
            return await _bookRepository.GetStatisticsAsync();
        }

        public async Task<BookResult> AddAsync(CreateBookDto dto)
        {
            var book = new Book(
                dto.Name,
                dto.Author,
                dto.ISBN,
                dto.PublisherId,
                dto.BookTypeId,
                dto.GenreId,
                dto.ConditionId,
                dto.Price,
                dto.Quantity,
                dto.Year,
                dto.Summary);

            await _bookRepository.AddAsync(book);

            return await SaveAsync(book, dto.CoverImage, dto.CoverImageFileName);
        }

        public async Task<BookResult> UpdateAsync(UpdateBookDto dto)
        {
            var book = await _bookRepository.GetAsync(dto.BookId);

            book.Name = dto.Name;
            book.Author = dto.Author;
            book.ISBN = dto.ISBN;
            book.PublisherId = dto.PublisherId;
            book.BookTypeId = dto.BookTypeId;
            book.GenreId = dto.GenreId;
            book.ConditionId = dto.ConditionId;
            book.Price = dto.Price;
            book.Quantity = dto.Quantity;
            book.Year = dto.Year;
            book.Summary = dto.Summary;
            book.UpdatedOn = DateTime.UtcNow;

            await _bookRepository.UpdateAsync(book);

            return await SaveAsync(book, dto.CoverImage, dto.CoverImageFileName);
        }

        private async Task<BookResult> SaveAsync(Book book, Stream? coverImage, string? coverImageFileName)
        {
            var resizedCoverImage = await ResizeImageAsync(coverImage);
            var imageIsSafe = await _imageValidationService.IsSafeAsync(resizedCoverImage ?? coverImage!);

            if (!imageIsSafe)
                return new BookResult(false, "The image failed the safety check. Please try another image.");

            await SaveImageAsync(book, resizedCoverImage, coverImageFileName);

            await _bookRepository.SaveChangesAsync();

            return new BookResult(true, null);
        }

        private async Task<Stream?> ResizeImageAsync(Stream? coverImage)
        {
            if (coverImage == null) return null;
            return await _imageResizeService.ResizeImageAsync(coverImage);
        }

        private async Task SaveImageAsync(Book book, Stream? coverImage, string? coverImageFilename)
        {
            if (coverImage == null) return;

            var imageUrl = await _fileService.SaveAsync(coverImage, coverImageFilename ?? string.Empty);

            await _fileService.DeleteAsync(book.CoverImageUrl ?? string.Empty);
            book.CoverImageUrl = imageUrl;
        }
    }
}
