using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Hubs;
using backend.Api.Models.DTOs.Order;
using backend.Api.Models.Entities;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Services.Interfaces;

namespace backend.Api.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IHubContext<RestaurantHub> _hubContext;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IMenuRepository menuRepository,
            IHubContext<RestaurantHub> hubContext,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _menuRepository = menuRepository;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        public async Task<OrderDto> GetOrderByIdAsync(string orderId)
        {
            var order = await _orderRepository.GetFirstOrDefaultAsync(
                o => o.Id == orderId,
                "Customer,PreparedBy,Items.MenuItem"
            );

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<List<OrderDto>> GetOrdersByCustomerIdAsync(string customerId)
        {
            var orders = await _orderRepository.GetAsync(
                o => o.CustomerId == customerId,
                query => query.OrderByDescending(o => o.CreatedAt),
                "Customer,Items.MenuItem"
            );

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<List<OrderDto>> GetActiveOrdersAsync()
        {
            var activeStatuses = new[] 
            { 
                OrderStatus.Placed, 
                OrderStatus.Preparing, 
                OrderStatus.Ready, 
                OrderStatus.Served 
            };

            var orders = await _orderRepository.GetAsync(
                o => activeStatuses.Contains(o.Status),
                query => query.OrderBy(o => o.CreatedAt),
                "Customer,Items.MenuItem"
            );

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetAsync(
                o => o.Status == status,
                query => query.OrderByDescending(o => o.CreatedAt),
                "Customer,Items.MenuItem"
            );

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // Make sure endDate includes the entire day
            endDate = endDate.Date.AddDays(1).AddTicks(-1);

            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate,
                query => query.OrderByDescending(o => o.CreatedAt),
                "Customer,Items.MenuItem"
            );

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<string> CreateOrderAsync(CreateOrderDto orderDto, string customerId)
        {
            var customer = await _userRepository.GetByIdAsync(customerId);
            if (customer == null)
                return null;

            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                Customer = customer,
                Status = OrderStatus.Placed,
                Type = orderDto.Type,
                TableNumber = orderDto.TableNumber,
                PaymentMethod = orderDto.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                SpecialInstructions = orderDto.SpecialInstructions,
                TipAmount = orderDto.TipAmount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create order items
            foreach (var itemDto in orderDto.Items)
            {
                var menuItem = await _menuRepository.GetByIdAsync(itemDto.MenuItemId);
                if (menuItem == null || !menuItem.IsAvailable)
                    continue;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = order.Id,
                    Order = order,
                    MenuItemId = menuItem.Id,
                    MenuItem = menuItem,
                    Quantity = itemDto.Quantity,
                    UnitPrice = menuItem.Price,
                    Subtotal = menuItem.Price * itemDto.Quantity,
                    Customizations = itemDto.Customizations,
                    SpecialInstructions = itemDto.SpecialInstructions
                };

                order.Items.Add(orderItem);
            }

            // Calculate order totals
            CalculateOrderTotals(order);

            // Set estimated wait time
            order.EstimatedWaitTimeMinutes = CalculateEstimatedWaitTime(order);

            // Save order
            await _orderRepository.AddAsync(order);

            // Notify staff about new order
            await _hubContext.Clients.Groups(new[] { "Staff", "Admin" }).SendAsync("NewOrder", order.Id);

            return order.Id;
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status, string staffId = null)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            // If completed, set completion time
            if (status == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.UtcNow;
            }

            // If preparing or ready, set prepared by
            if ((status == OrderStatus.Preparing || status == OrderStatus.Ready) && !string.IsNullOrEmpty(staffId))
            {
                order.PreparedById = staffId;
                order.PreparedBy = await _userRepository.GetByIdAsync(staffId);
            }

            var success = await _orderRepository.UpdateAsync(order);

            if (success)
            {
                // Notify customer and staff about status change
                await _hubContext.Clients.User(order.CustomerId).SendAsync("OrderStatusChanged", orderId, status);
                await _hubContext.Clients.Groups(new[] { "Staff", "Admin" }).SendAsync("OrderStatusChanged", orderId, status);
            }

            return success;
        }

        public async Task<bool> CancelOrderAsync(string orderId, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            // Check if order can be cancelled
            if (order.Status != OrderStatus.Placed && order.Status != OrderStatus.Preparing)
                return false;

            // For customers, check if it's their order
            var userRole = await GetUserRole(userId);
            if (userRole == "Customer" && order.CustomerId != userId)
                return false;

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;

            var success = await _orderRepository.UpdateAsync(order);

            if (success)
            {
                // Notify customer and staff about cancellation
                await _hubContext.Clients.User(order.CustomerId).SendAsync("OrderStatusChanged", orderId, OrderStatus.Cancelled);
                await _hubContext.Clients.Groups(new[] { "Staff", "Admin" }).SendAsync("OrderStatusChanged", orderId, OrderStatus.Cancelled);
            }

            return success;
        }

        public async Task<int> GetEstimatedWaitTimeAsync(string orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return 0;

            return order.EstimatedWaitTimeMinutes;
        }

        public async Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod, PaymentStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            order.PaymentMethod = paymentMethod;
            order.PaymentStatus = status;
            order.UpdatedAt = DateTime.UtcNow;

            return await _orderRepository.UpdateAsync(order);
        }

        public async Task<List<OrderSummaryDto>> GetOrderSummariesAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);

            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate,
                query => query.OrderByDescending(o => o.CreatedAt),
                "Customer,Items"
            );

            return _mapper.Map<List<OrderSummaryDto>>(orders);
        }

        private void CalculateOrderTotals(Order order)
        {
            order.Subtotal = order.Items.Sum(i => i.Subtotal);
            order.Tax = Math.Round(order.Subtotal * 0.10m, 2); // Assuming 10% tax
            order.DiscountAmount = 0; // No discount logic implemented yet
            order.TotalAmount = order.Subtotal + order.Tax - order.DiscountAmount + order.TipAmount;
        }

        private int CalculateEstimatedWaitTime(Order order)
        {
            // Simple algorithm: 5 min base + 2 min per item
            int baseTime = 5;
            int itemTime = order.Items.Sum(i => i.Quantity) * 2;
            return baseTime + itemTime;
        }

        private async Task<string> GetUserRole(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.UserType.ToString() ?? "Customer";
        }
    }
}