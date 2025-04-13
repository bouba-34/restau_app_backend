using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Api.Models.Entities;
using backend.Api.Models.Responses;
using backend.Api.Services.Interfaces;

namespace backend.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("daily/{date}")]
        public async Task<IActionResult> GetDailySalesReport(DateTime date)
        {
            var report = await _reportService.GetDailySalesReportAsync(date);
            return Ok(ApiResponse<SalesReport>.SuccessResponse(report));
        }

        [HttpGet("weekly/{startDate}")]
        public async Task<IActionResult> GetWeeklySalesReport(DateTime startDate)
        {
            var report = await _reportService.GetWeeklySalesReportAsync(startDate);
            return Ok(ApiResponse<SalesReport>.SuccessResponse(report));
        }

        [HttpGet("monthly/{year}/{month}")]
        public async Task<IActionResult> GetMonthlySalesReport(int year, int month)
        {
            var report = await _reportService.GetMonthlySalesReportAsync(year, month);
            return Ok(ApiResponse<SalesReport>.SuccessResponse(report));
        }

        [HttpGet("top-selling")]
        public async Task<IActionResult> GetTopSellingItems([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int limit = 10)
        {
            var items = await _reportService.GetTopSellingItemsAsync(startDate, endDate, limit);
            return Ok(ApiResponse<List<MenuItemSales>>.SuccessResponse(items));
        }

        [HttpGet("sales-by-category")]
        public async Task<IActionResult> GetSalesByCategory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var salesByCategory = await _reportService.GetSalesByCategoryAsync(startDate, endDate);
            return Ok(ApiResponse<Dictionary<string, decimal>>.SuccessResponse(salesByCategory));
        }

        [HttpGet("orders-by-hour/{date}")]
        public async Task<IActionResult> GetOrdersByHour(DateTime date)
        {
            var ordersByHour = await _reportService.GetOrdersByHourAsync(date);
            return Ok(ApiResponse<Dictionary<int, int>>.SuccessResponse(ordersByHour));
        }

        [HttpGet("sales-by-payment")]
        public async Task<IActionResult> GetSalesByPaymentMethod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var salesByPayment = await _reportService.GetSalesByPaymentMethodAsync(startDate, endDate);
            return Ok(ApiResponse<Dictionary<PaymentMethod, decimal>>.SuccessResponse(salesByPayment));
        }

        [HttpGet("export/sales")]
        public async Task<IActionResult> ExportSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "csv")
        {
            var fileResult = await _reportService.ExportSalesReportAsync(startDate, endDate, format);
            
            if (fileResult == null)
                return BadRequest(new ErrorResponse("Failed to generate export"));
                
            return File(fileResult.FileContent, fileResult.ContentType, fileResult.FileName);
        }

        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var summary = await _reportService.GetDashboardSummaryAsync();
            return Ok(ApiResponse<object>.SuccessResponse(summary));
        }
    }
    
    public class SalesReport
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public Dictionary<string, decimal> SalesByCategory { get; set; } = new Dictionary<string, decimal>();
        public List<MenuItemSales> TopSellingItems { get; set; } = new List<MenuItemSales>();
        public Dictionary<int, int> OrdersByHour { get; set; } = new Dictionary<int, int>();
        public Dictionary<PaymentMethod, decimal> SalesByPaymentMethod { get; set; } = new Dictionary<PaymentMethod, decimal>();
    }

    public class MenuItemSales
    {
        public string MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalSales { get; set; }
    }
    
    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        MobilePayment,
        GiftCard,
        OnlinePayment
    }
}