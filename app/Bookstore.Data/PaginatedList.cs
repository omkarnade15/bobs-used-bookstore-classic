using Microsoft.EntityFrameworkCore;

namespace Bookstore.Domain
{
    public class PaginatedList<T> : List<T>, IPaginatedList<T> where T : Entity
    {
        private readonly IQueryable<T> _source;
        private readonly int _pageIndex;
        private readonly int _pageSize;

        public int PageIndex { get; private set; }

        public int TotalPages { get; private set; }

        private PaginatedList() { _source = null!; }

        public PaginatedList(IQueryable<T> source, int pageIndex, int pageSize)
        {
            _source = source;
            _pageIndex = pageIndex;
            _pageSize = pageSize;
        }

        public async Task PopulateAsync()
        {
            var count = await _source.CountAsync();
            var items = await _source.OrderBy(x => x.Id).Skip((_pageIndex - 1) * _pageSize).Take(_pageSize).ToListAsync();

            PageIndex = _pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)_pageSize);

            AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public IEnumerable<int> GetPageList(int count)
        {
            var pagesCount = 1;
            var newPagesCount = 1;
            var start = PageIndex;
            var end = PageIndex;

            while (pagesCount < count)
            {
                if (end + 1 <= TotalPages)
                {
                    end++;
                    newPagesCount++;
                }

                if (start - 1 > 0)
                {
                    start--;
                    newPagesCount++;
                }

                if (newPagesCount == pagesCount)
                    break;
                else
                    pagesCount = newPagesCount;
            }

            return Enumerable.Range(start, pagesCount);
        }
    }
}
