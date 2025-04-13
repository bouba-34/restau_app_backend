using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId);
        Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status);
        Task<bool> UpdatePaymentStatusAsync(string orderId, PaymentStatus status);
    }
}