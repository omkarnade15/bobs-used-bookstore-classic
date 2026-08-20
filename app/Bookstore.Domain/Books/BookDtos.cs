namespace Bookstore.Domain.Books
{
    public class CreateBookDto
    {
        public CreateBookDto(
            string name,
            string author,
            int bookTypeId,
            int conditionId,
            int genreId,
            int publisherId,
            int? year,
            string isbn,
            string? summary,
            decimal price,
            int quantity,
            Stream? coverImage,
            string? coverImageFileName)
        {
            Name = name;
            Author = author;
            BookTypeId = bookTypeId;
            ConditionId = conditionId;
            GenreId = genreId;
            PublisherId = publisherId;
            Year = year;
            ISBN = isbn;
            Summary = summary;
            Price = price;
            Quantity = quantity;
            CoverImage = coverImage;
            CoverImageFileName = coverImageFileName;
        }

        public string Name { get; }
        public string Author { get; }
        public int BookTypeId { get; }
        public int ConditionId { get; }
        public int GenreId { get; }
        public int PublisherId { get; }
        public int? Year { get; }
        public string ISBN { get; }
        public string? Summary { get; }
        public decimal Price { get; }
        public int Quantity { get; }
        public Stream? CoverImage { get; }
        public string? CoverImageFileName { get; }
    }

    public class UpdateBookDto
    {
        public UpdateBookDto(
            int bookId,
            string name,
            string author,
            int bookTypeId,
            int conditionId,
            int genreId,
            int publisherId,
            int? year,
            string isbn,
            string? summary,
            decimal price,
            int quantity,
            Stream? coverImage,
            string? coverImageFileName)
        {
            BookId = bookId;
            Name = name;
            Author = author;
            BookTypeId = bookTypeId;
            ConditionId = conditionId;
            GenreId = genreId;
            PublisherId = publisherId;
            Year = year;
            ISBN = isbn;
            Summary = summary;
            Price = price;
            Quantity = quantity;
            CoverImage = coverImage;
            CoverImageFileName = coverImageFileName;
        }

        public int BookId { get; }
        public string Name { get; }
        public string Author { get; }
        public int BookTypeId { get; }
        public int ConditionId { get; }
        public int GenreId { get; }
        public int PublisherId { get; }
        public int? Year { get; }
        public string ISBN { get; }
        public string? Summary { get; }
        public decimal Price { get; }
        public int Quantity { get; }
        public Stream? CoverImage { get; }
        public string? CoverImageFileName { get; }
    }
}
