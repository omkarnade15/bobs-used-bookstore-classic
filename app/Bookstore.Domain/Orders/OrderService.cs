using Bookstore.Domain.Carts;
using Bookstore.Domain.Customers;

namespace Bookstore.Domain.Orders
{
    public interface IOrderService
    {
        Task<IPaginatedList<Order>> GetOrdersAsync(OrderFilters filters, int pageIndex = 1, int pageSize = 10);

        Task<IEnumerable<Order>> GetOrdersAsync(string sub);

        Task<Order?> GetOrderAsync(int id);

        Task<OrderStatistics> GetStatisticsAsync();

        Task<int> CreateOrderAsync(CreateOrderDto createOrderDto);

        Task UpdateOrderStatusAsync(UpdateOrderStatusDto updateOrderStatusDto);

        Task CancelOrderAsync(CancelOrderDto cancelOrderDto);
    }

    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IShoppingCartRepository shoppingCartRepository,
            ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _shoppingCartRepository = shoppingCartRepository;
            _customerRepository = customerRepository;
        }

        public async Task<IPaginatedList<Order>> GetOrdersAsync(OrderFilters filters, int pageIndex = 1, int pageSize = 10)
        {
            return await _orderRepository.ListAsync(filters, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(string sub)
        {
            return await _orderRepository.ListAsync(sub);
        }

        public async Task<Order?> GetOrderAsync(int id)
        {
            return await _orderRepository.GetAsync(id);
        }

        public async Task<OrderStatistics> GetStatisticsAsync()
        {
            return await _orderRepository.GetStatisticsAsync();
        }

        public async Task<int> CreateOrderAsync(CreateOrderDto dto)
        {
            var shoppingCart = await _shoppingCartRepository.GetAsync(dto.CorrelationId)
                ?? throw new InvalidOperationException("Shopping cart not found.");

            var customer = await _customerRepository.GetAsync(dto.CustomerSub)
                ?? throw new InvalidOperationException($"Customer with sub '{dto.CustomerSub}' not found.");

            var order = new Order(customer.Id, dto.AddressId);

            await _orderRepository.AddAsync(order);

            shoppingCart.GetShoppingCartItems(ShoppingCartItemFilter.ExcludeOutOfStockItems).ToList().ForEach(x =>
            {
                order.AddOrderItem(x.Book, x.Quantity);
                x.Book.ReduceStockLevel(x.Quantity);
                shoppingCart.RemoveShoppingCartItemById(x.Id);
            });

            await _orderRepository.SaveChangesAsync();

            return order.Id;
        }

        public async Task UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetAsync(dto.OrderId);
            if (order == null) return;

            order.OrderStatus = dto.OrderStatus;
            order.UpdatedOn = DateTime.UtcNow;

            await _orderRepository.SaveChangesAsync();
        }

        public async Task CancelOrderAsync(CancelOrderDto dto)
        {
            var order = await _orderRepository.GetAsync(dto.OrderId, dto.CustomerSub);
            if (order == null) return;

            order.OrderStatus = OrderStatus.Cancelled;

            await _orderRepository.SaveChangesAsync();
        }
    }
}
