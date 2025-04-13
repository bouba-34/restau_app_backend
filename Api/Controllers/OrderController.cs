using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.DTOs.Order;
using backend.Api.Models.Entities;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
                return NotFound(new ErrorResponse("Order not found"));
                
            return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetOrdersByCustomerId(string customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetActiveOrders()
        {
            var orders = await _orderService.GetActiveOrdersAsync();
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        [HttpGet("date-range")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOrdersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorResponse("User not authenticated"));
                
            var orderId = await _orderService.CreateOrderAsync(orderDto, userId);
            
            if (string.IsNullOrEmpty(orderId))
                return BadRequest(new ErrorResponse("Failed to create order"));
                
            var order = await _orderService.GetOrderByIdAsync(orderId);
            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, ApiResponse<OrderDto>.SuccessResponse(order, "Order created successfully"));
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var success = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status, userId);
            
            if (!success)
                return NotFound(new ErrorResponse("Order not found or status update failed"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Order status updated successfully"));
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(string id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var success = await _orderService.CancelOrderAsync(id, userId);
            
            if (!success)
                return NotFound(new ErrorResponse("Order not found or cannot be cancelled"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Order cancelled successfully"));
        }

        [HttpGet("{id}/wait-time")]
        public async Task<IActionResult> GetEstimatedWaitTime(string id)
        {
            var waitTime = await _orderService.GetEstimatedWaitTimeAsync(id);
            return Ok(ApiResponse<int>.SuccessResponse(waitTime));
        }

        [HttpPost("{id}/payment")]
        public async Task<IActionResult> ProcessPayment(string id, [FromBody] UpdatePaymentStatusDto paymentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse("Invalid model", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
                
            var success = await _orderService.ProcessPaymentAsync(id, paymentDto.PaymentMethod, paymentDto.Status);
            
            if (!success)
                return NotFound(new ErrorResponse("Order not found or payment processing failed"));
                
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Payment processed successfully"));
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetOrderSummaries([FromQuery] DateTime? date = null)
        {
            date ??= DateTime.Today;
            var summaries = await _orderService.GetOrderSummariesAsync(date.Value);
            return Ok(ApiResponse<List<OrderSummaryDto>>.SuccessResponse(summaries));
        }
    }
}