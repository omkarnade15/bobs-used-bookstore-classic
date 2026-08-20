using Amazon.Rekognition;
using Amazon.S3;
using BobsBookstoreClassic.Data;
using Bookstore.Data;
using Bookstore.Data.FileServices;
using Bookstore.Data.ImageResizeService;
using Bookstore.Data.ImageValidationServices;
using Bookstore.Data.Repositories;
using Bookstore.Domain;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Books;
using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Orders;
using Bookstore.Domain.ReferenceData;
using Bookstore.Web.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Web
{
    public static class DependencyInjectionSetup
    {
        public static void ConfigureDependencyInjection(IServiceCollection services, IWebHostEnvironment env)
        {
            // EF Core DbContext
            var connectionString = BookstoreConfiguration.GetConnectionString("BookstoreDatabaseConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Domain services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReferenceDataService, ReferenceDataService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IShoppingCartService, ShoppingCartService>();
            services.AddScoped<IImageResizeService, ImageResizeService>();

            // Repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IOfferRepository, OfferRepository>();
            services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
            services.AddScoped(typeof(IPaginatedList<>), typeof(PaginatedList<>));

            // File service
            if (BookstoreConfiguration.GetSetting("Services/FileService") == "aws")
            {
                services.AddSingleton<IAmazonS3, AmazonS3Client>();
                services.AddScoped<IFileService, S3FileService>();
            }
            else
            {
                var webRootPath = env.WebRootPath ?? env.ContentRootPath;
                services.AddSingleton<IFileService>(new LocalFileService(webRootPath));
            }

            // Image validation
            if (BookstoreConfiguration.GetSetting("Services/ImageValidationService") == "aws")
            {
                services.AddSingleton<IAmazonRekognition, AmazonRekognitionClient>();
                services.AddScoped<IImageValidationService, RekognitionImageValidationService>();
            }
            else
            {
                services.AddScoped<IImageValidationService, LocalImageValidationService>();
            }

            // Local auth middleware (only registered for local auth scenario)
            if (BookstoreConfiguration.GetSetting("Services/Authentication") != "aws")
            {
                services.AddScoped<LocalAuthenticationMiddleware>();
            }
        }
    }
}
