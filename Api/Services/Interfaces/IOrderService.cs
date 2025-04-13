using backend.Api.Models.DTOs.Order;
using backend.Api.Models.Entities;

namespace backend.Api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderByIdAsync(string orderId);
        Task<List<OrderDto>> GetOrdersByCustomerIdAsync(string customerId);
        Task<List<OrderDto>> GetActiveOrdersAsync();
        Task<List<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<string> CreateOrderAsync(CreateOrderDto orderDto, string customerId);
        Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status, string staffId = null);
        Task<bool> CancelOrderAsync(string orderId, string userId);
        Task<int> GetEstimatedWaitTimeAsync(string orderId);
        Task<bool> ProcessPaymentAsync(string orderId, string paymentMethod, PaymentStatus status);
        Task<List<OrderSummaryDto>> GetOrderSummariesAsync(DateTime date);
    }
}