using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IOrderRepo : IGenericRepository<Order, int>
    {
        Task<IEnumerable<Order>> GetUserOrders(int id);

        Task<bool> HasUserReceivedProductAsync(int userId, int productId, CancellationToken ct = default);
        Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default);
        Task<(IReadOnlyList<Order> Items, int TotalCount)> GetOrdersPagedAsync(int pageNumber, int pageSize, OrderSortBy sortBy, bool descending, int? branchId, CancellationToken ct = default);
        Task<Order> GetOrderViaInvoiceNumber(string InvoiceNumber);
        Task<Order?> GetFullOrderDetailsAsync(int id, CancellationToken ct = default);
        Task<Order?> GetOrderItemsForUserAsync(int orderId, int userId, CancellationToken ct = default);
        Task<int> GetTotalOrdersCountAsync(int? branchId, CancellationToken ct = default);
        Task<int> GetCompletedOrdersCountAsync(int? branchId, CancellationToken ct = default);
        Task<int> GetPendingOrdersCountAsync(int? branchId, CancellationToken ct = default);
        Task<decimal> GetTotalRevenueAsync(int? branchId, CancellationToken ct = default);
        Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPosSalesPagedAsync(
            int pageNumber, int pageSize,
            string? search,
            OrderStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? branchId,
            CancellationToken ct = default);

        Task<Order?> GetPosOrderDetailsAsync(int id, CancellationToken ct = default);


        // ═══════════════════════════════════════════════════════════════
        // SALES OVERVIEW & DASHBOARD
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns comprehensive sales overview with month/day comparisons,
        /// online completed orders (Web + App), POS sales, and discount revenue.
        /// </summary>
        Task<(
       int CurrentMonthSales, int PreviousMonthSales, // Changed from decimal to int
       int CurrentMonthOrders, int PreviousMonthOrders,
       decimal CurrentMonthRevenue, decimal PreviousMonthRevenue,
       int TodayOrders, int YesterdayOrders,
       int CompletedWebOrders, decimal WebRevenue,
       int PosOrdersCount, decimal PosRevenue,
       decimal TotalDiscountRevenue
   )> GetSalesOverviewRawAsync(int? branchId, CancellationToken ct = default);
        /// <summary>
        /// Returns completed online orders count and revenue for Website and MobileApp.
        /// </summary>
        Task<(int CompletedWebsiteOrders, decimal WebsiteRevenue)>
        GetCompletedOnlineOrdersAsync(int? branchId, CancellationToken ct = default);

        /// <summary>
        /// Returns POS orders count and total POS revenue.
        /// </summary>
        Task<(int PosOrdersCount, decimal PosRevenue)>
            GetPosSalesAsync(int? branchId, CancellationToken ct = default);

        /// <summary>
        /// Returns revenue breakdown by source (POS, Website, MobileApp) for completed orders only.
        /// </summary>
        Task<(decimal PosRevenue, decimal WebsiteRevenue)>
       GetRevenueBySourceAsync(int? branchId, CancellationToken ct = default);

        /// <summary>
        /// Returns total discount amount across all orders.
        /// </summary>
        Task<decimal> GetTotalDiscountRevenueAsync(int? branchId, CancellationToken ct = default);

        // ═══════════════════════════════════════════════════════════════
        // DISTRIBUTION & ANALYTICS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns order count and total value grouped by status.
        /// </summary>
        Task<List<(OrderStatus Status, int Count, decimal TotalValue)>>
            GetOrderStatusDistributionAsync(int? branchId, CancellationToken ct = default);

        /// <summary>
        /// Returns order count and total value grouped by source.
        /// </summary>
        Task<List<(OrderSource Source, int Count, decimal TotalValue)>>
            GetOrdersBySourceDistributionAsync(int? branchId, CancellationToken ct = default);

        // ═══════════════════════════════════════════════════════════════
        // TOP PERFORMERS
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns top customers ordered by order count, then by total spent.
        /// </summary>
        Task<List<(int CustomerId, string FullName, int OrderCount, decimal TotalSpent)>>
            GetTopCustomersAsync(int topCount, int? branchId, CancellationToken ct = default);

        /// <summary>
        /// Returns top branches ordered by total revenue, then by order count.
        /// </summary>
        Task<List<(int BranchId, string BranchName, int OrderCount, decimal TotalRevenue)>>
            GetTopBranchesAsync(int topCount, CancellationToken ct = default);

        /// <summary>
        /// Returns top selling products by quantity sold (completed orders only).
        /// </summary>
        Task<List<(int ProductId, string ProductName, int QuantitySold)>>
            GetTopSellingProductsAsync(int topCount, int? branchId, CancellationToken ct = default);

        /// <summary>
        /// Returns count of best-selling (above average) and least-selling (below average) products.
        /// </summary>
        Task<(int BestSellingCount, int LeastSellingCount)>
            GetBestAndLeastSellingProductCountsAsync(int? branchId, CancellationToken ct = default);

        // ═══════════════════════════════════════════════════════════════
        // ORDERS LISTING
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns paginated latest orders ordered by creation date descending.
        /// </summary>
        Task<(IReadOnlyList<Order> Items, int TotalCount)>
            GetLatestOrdersPagedAsync(int pageNumber, int pageSize, int? branchId, CancellationToken ct = default);
    }


}