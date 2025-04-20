using backend.Api.Data.Repositories.Interfaces;
using backend.Api.Models.Entities;
using backend.Api.Services.Interfaces;
using System.Text;
using System.Text.Json;
using backend.Api.Controllers;

namespace backend.Api.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuRepository _menuRepository;

        public ReportService(
            IOrderRepository orderRepository,
            IMenuRepository menuRepository)
        {
            _orderRepository = orderRepository;
            _menuRepository = menuRepository;
        }

        public async Task<SalesReport> GetDailySalesReportAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);

            return await GenerateReportForDateRange(startDate, endDate);
        }

        public async Task<SalesReport> GetWeeklySalesReportAsync(DateTime startDate)
        {
            // Ensure we start from Sunday or Monday based on culture
            startDate = startDate.Date;
            var endDate = startDate.AddDays(7).AddTicks(-1);

            return await GenerateReportForDateRange(startDate, endDate);
        }

        public async Task<SalesReport> GetMonthlySalesReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            return await GenerateReportForDateRange(startDate, endDate);
        }

        public async Task<List<MenuItemSales>> GetTopSellingItemsAsync(DateTime startDate, DateTime endDate, int limit = 10)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
            
            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Completed,
                null,
                "Items.MenuItem"
            );

            var itemSales = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.MenuItemId)
                .Select(g => new MenuItemSales
                {
                    MenuItemId = g.Key,
                    MenuItemName = g.First().MenuItem.Name,
                    QuantitySold = g.Sum(i => i.Quantity),
                    TotalSales = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(limit)
                .ToList();

            return itemSales;
        }

        public async Task<Dictionary<string, decimal>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
            
            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && o.Status == OrderStatus.Completed,
                null,
                "Items.MenuItem.Category"
            );

            var salesByCategory = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.MenuItem.Category.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(i => i.Subtotal)
                );

            return salesByCategory;
        }

        public async Task<Dictionary<int, int>> GetOrdersByHourAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1).AddTicks(-1);
            
            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate,
                null,
                ""
            );

            var ordersByHour = Enumerable.Range(0, 24)
                .ToDictionary(
                    hour => hour,
                    hour => orders.Count(o => o.CreatedAt.Hour == hour)
                );

            return ordersByHour;
        }

        public async Task<Dictionary<PaymentMethod, decimal>> GetSalesByPaymentMethodAsync(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
            
            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate && 
                     o.Status == OrderStatus.Completed && o.PaymentStatus == PaymentStatus.Paid,
                null,
                ""
            );

            // Convert string payment methods to enum
            var salesByPaymentMethod = new Dictionary<PaymentMethod, decimal>();
            
            foreach (var paymentMethod in Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>())
            {
                salesByPaymentMethod[paymentMethod] = orders
                    .Where(o => o.PaymentMethod == paymentMethod.ToString())
                    .Sum(o => o.TotalAmount);
            }

            return salesByPaymentMethod;
        }

        public async Task<FileResult> ExportSalesReportAsync(DateTime startDate, DateTime endDate, string format = "csv")
        {
            var report = await GenerateReportForDateRange(startDate, endDate);
            
            switch (format.ToLower())
            {
                case "csv":
                    return ExportReportToCsv(report, startDate, endDate);
                    
                case "json":
                    return ExportReportToJson(report, startDate, endDate);
                    
                default:
                    return ExportReportToCsv(report, startDate, endDate);
            }
        }

        public async Task<object> GetDashboardSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
            var lastWeekStart = thisWeekStart.AddDays(-7);
            var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc); // Ajout du Kind

            var todayReport = await GetDailySalesReportAsync(today);
            var yesterdayReport = await GetDailySalesReportAsync(yesterday);
            var thisWeekReport = await GenerateReportForDateRange(thisWeekStart, today);
            var lastWeekReport = await GenerateReportForDateRange(lastWeekStart, thisWeekStart.AddDays(-1));
            var thisMonthReport = await GenerateReportForDateRange(thisMonthStart, today);

            var pendingOrders = await _orderRepository.CountAsync(o => 
                o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

            var topItems = await GetTopSellingItemsAsync(thisWeekStart, today, 5);

            return new
            {
                Today = new
                {
                    TotalSales = todayReport.TotalSales,
                    TotalOrders = todayReport.TotalOrders,
                    AverageOrderValue = todayReport.AverageOrderValue
                },
                Yesterday = new
                {
                    TotalSales = yesterdayReport.TotalSales,
                    TotalOrders = yesterdayReport.TotalOrders,
                    AverageOrderValue = yesterdayReport.AverageOrderValue
                },
                ThisWeek = new
                {
                    TotalSales = thisWeekReport.TotalSales,
                    TotalOrders = thisWeekReport.TotalOrders,
                    AverageOrderValue = thisWeekReport.AverageOrderValue
                },
                LastWeek = new
                {
                    TotalSales = lastWeekReport.TotalSales,
                    TotalOrders = lastWeekReport.TotalOrders,
                    AverageOrderValue = lastWeekReport.AverageOrderValue
                },
                ThisMonth = new
                {
                    TotalSales = thisMonthReport.TotalSales,
                    TotalOrders = thisMonthReport.TotalOrders,
                    AverageOrderValue = thisMonthReport.AverageOrderValue
                },
                PendingOrders = pendingOrders,
                TopSellingItems = topItems,
                SalesByCategory = thisWeekReport.SalesByCategory,
                OrdersByHour = todayReport.OrdersByHour
            };
        }


        private async Task<SalesReport> GenerateReportForDateRange(DateTime startDate, DateTime endDate)
        {
            var orders = await _orderRepository.GetAsync(
                o => o.CreatedAt >= startDate && o.CreatedAt <= endDate,
                null,
                "Items.MenuItem.Category"
            );

            var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
            
            var totalSales = completedOrders.Sum(o => o.TotalAmount);
            var totalOrders = completedOrders.Count;
            var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

            // Get sales by category
            var salesByCategory = completedOrders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.MenuItem.Category.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(i => i.Subtotal)
                );

            // Get top selling items
            var topSellingItems = completedOrders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.MenuItemId)
                .Select(g => new MenuItemSales
                {
                    MenuItemId = g.Key,
                    MenuItemName = g.First().MenuItem.Name,
                    QuantitySold = g.Sum(i => i.Quantity),
                    TotalSales = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToList();

            // Get orders by hour
            var ordersByHour = Enumerable.Range(0, 24)
                .ToDictionary(
                    hour => hour,
                    hour => orders.Count(o => o.CreatedAt.Hour == hour)
                );

            // Get sales by payment method
            var salesByPaymentMethod = new Dictionary<PaymentMethod, decimal>();
            
            foreach (var paymentMethod in Enum.GetValues(typeof(PaymentMethod)).Cast<PaymentMethod>())
            {
                salesByPaymentMethod[paymentMethod] = completedOrders
                    .Where(o => o.PaymentMethod == paymentMethod.ToString())
                    .Sum(o => o.TotalAmount);
            }

            return new SalesReport
            {
                Date = startDate,
                TotalSales = totalSales,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                SalesByCategory = salesByCategory,
                TopSellingItems = topSellingItems,
                OrdersByHour = ordersByHour,
                SalesByPaymentMethod = salesByPaymentMethod
            };
        }

        private FileResult ExportReportToCsv(SalesReport report, DateTime startDate, DateTime endDate)
        {
            var csv = new StringBuilder();
            
            // Add header
            csv.AppendLine("Sales Report");
            csv.AppendLine($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            csv.AppendLine();
            
            // Add summary
            csv.AppendLine("Summary");
            csv.AppendLine($"Total Sales,{report.TotalSales:C}");
            csv.AppendLine($"Total Orders,{report.TotalOrders}");
            csv.AppendLine($"Average Order Value,{report.AverageOrderValue:C}");
            csv.AppendLine();
            
            // Add sales by category
            csv.AppendLine("Sales by Category");
            csv.AppendLine("Category,Amount");
            foreach (var category in report.SalesByCategory)
            {
                csv.AppendLine($"{category.Key},{category.Value:C}");
            }
            csv.AppendLine();
            
            // Add top selling items
            csv.AppendLine("Top Selling Items");
            csv.AppendLine("Item,Quantity Sold,Total Sales");
            foreach (var item in report.TopSellingItems)
            {
                csv.AppendLine($"{item.MenuItemName},{item.QuantitySold},{item.TotalSales:C}");
            }
            csv.AppendLine();
            
            // Add orders by hour
            csv.AppendLine("Orders by Hour");
            csv.AppendLine("Hour,Count");
            foreach (var hour in report.OrdersByHour.OrderBy(h => h.Key))
            {
                csv.AppendLine($"{hour.Key:00}:00,{hour.Value}");
            }
            csv.AppendLine();
            
            // Add sales by payment method
            csv.AppendLine("Sales by Payment Method");
            csv.AppendLine("Payment Method,Amount");
            foreach (var payment in report.SalesByPaymentMethod)
            {
                csv.AppendLine($"{payment.Key},{payment.Value:C}");
            }

            return new FileResult
            {
                FileContent = Encoding.UTF8.GetBytes(csv.ToString()),
                ContentType = "text/csv",
                FileName = $"sales_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv"
            };
        }

        private FileResult ExportReportToJson(SalesReport report, DateTime startDate, DateTime endDate)
        {
            var reportObject = new
            {
                reportPeriod = new
                {
                    startDate = startDate,
                    endDate = endDate
                },
                summary = new
                {
                    totalSales = report.TotalSales,
                    totalOrders = report.TotalOrders,
                    averageOrderValue = report.AverageOrderValue
                },
                salesByCategory = report.SalesByCategory,
                topSellingItems = report.TopSellingItems,
                ordersByHour = report.OrdersByHour,
                salesByPaymentMethod = report.SalesByPaymentMethod
            };

            var json = JsonSerializer.Serialize(reportObject, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return new FileResult
            {
                FileContent = Encoding.UTF8.GetBytes(json),
                ContentType = "application/json",
                FileName = $"sales_report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.json"
            };
        }
    }
}