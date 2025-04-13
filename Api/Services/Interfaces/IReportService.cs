using backend.Api.Models.Entities;

namespace backend.Api.Services.Interfaces
{
    public interface IReportService
    {
        Task<SalesReport> GetDailySalesReportAsync(DateTime date);
        Task<SalesReport> GetWeeklySalesReportAsync(DateTime startDate);
        Task<SalesReport> GetMonthlySalesReportAsync(int year, int month);
        Task<List<MenuItemSales>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int limit = 10);
        Task<Dictionary<string, decimal>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<int, int>> GetOrdersByHourAsync(DateTime date);
        Task<Dictionary<PaymentMethod, decimal>> GetSalesByPaymentMethodAsync(DateTime startDate, DateTime endDate);
        Task<FileResult> ExportSalesReportAsync(DateTime startDate, DateTime endDate, string format = "csv");
        Task<object> GetDashboardSummaryAsync();
    }

    public class FileResult
    {
        public byte[] FileContent { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}