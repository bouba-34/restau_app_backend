using Microsoft.EntityFrameworkCore;
using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;

namespace backend.Api.Data.Repositories.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.MenuItem)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status)
        {
            var order = await _dbSet.FindAsync(orderId);
            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdatePaymentStatusAsync(string orderId, PaymentStatus status)
        {
            var order = await _dbSet.FindAsync(orderId);
            if (order == null)
                return false;

            order.PaymentStatus = status;
            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Order> GetByIdAsync(string id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.PreparedBy)
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                        .ThenInclude(m => m.Category)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.MenuItem)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}