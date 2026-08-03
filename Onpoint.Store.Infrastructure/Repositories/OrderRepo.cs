using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;
using System.Linq.Expressions;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class OrderRepo : GenericRepository<Order, int>, IOrderRepo
    {
        public OrderRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetPosOrderDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.Invoice)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.StatusHistory)
                .Include(o => o.Refunds)
                    .ThenInclude(r => r.RefundItems)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }
        public async Task<Order?> GetOrderItemsForUserAsync(int orderId, int userId, CancellationToken ct = default)
        {
            return await _dbset
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.AttributeValues)
                            .ThenInclude(av => av.ProductAttribute)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
        }
        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPosSalesPagedAsync(
            int pageNumber, int pageSize,
            string? search,
            OrderStatus? status,
            PaymentMethod? paymentMethod,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? branchId,
            CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Where(o => o.Source == OrderSource.Pos);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            if (paymentMethod.HasValue)
                query = query.Where(o => o.PaymentMethod == paymentMethod.Value);

            if (dateFrom.HasValue)
                query = query.Where(o => o.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(o => o.CreatedAt <= dateTo.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o =>
                    (o.InvoiceNumber != null && o.InvoiceNumber.ToLower().Contains(term)) ||
                    (o.Customer != null && (
                        (o.Customer.FName != null && o.Customer.FName.ToLower().Contains(term)) ||
                        (o.Customer.LName != null && o.Customer.LName.ToLower().Contains(term))
                    )) ||
                    (o.Cashier != null && (
                        o.Cashier.FirstName.ToLower().Contains(term) ||
                        o.Cashier.UserName.ToLower().Contains(term)
                    ))
                );
            }

            int totalCount = await query.CountAsync(ct);

            query = query.OrderByDescending(o => o.CreatedAt);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Order?> GetFullOrderDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)

                .Include(o => o.StatusHistory.OrderBy(sh => sh.EventTime))
                .Include(o => o.Refunds)
                    .ThenInclude(r => r.RefundItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.Source == OrderSource.Pos, ct);
        }

        public async Task<IEnumerable<Order>> GetUserOrders(int userId)
        {
            var orders = await _dbset
                 .Include(o => o.OrderItems)
                     .ThenInclude(oi => oi.Product)
                 .Where(o => o.UserId == userId)
                 .OrderByDescending(o => o.CreatedAt)
                 .ToListAsync();
            return orders;
        }


        public async Task<bool> HasUserReceivedProductAsync(int userId, int productId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Pending)
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => oi.ProductId == productId, ct);
        }
        public async Task<Order?> GetOrderWithItemsAsync(int orderId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }
        public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetOrdersPagedAsync(int pageNumber, int pageSize, OrderSortBy sortBy, bool descending, int? branchId, CancellationToken ct = default)
        {
            Expression<Func<Order, bool>>? predicate = branchId.HasValue
                ? o => o.BranchId == branchId.Value
                : null;

            Func<IQueryable<Order>, IOrderedQueryable<Order>> orderBy = sortBy switch
            {
                OrderSortBy.OrderNumber => q => descending ? q.OrderByDescending(o => o.Id) : q.OrderBy(o => o.Id),
                OrderSortBy.TotalAmount => q => descending ? q.OrderByDescending(o => o.TotalAmount) : q.OrderBy(o => o.TotalAmount),
                OrderSortBy.Status => q => descending ? q.OrderByDescending(o => o.Status) : q.OrderBy(o => o.Status),
                _ => q => descending ? q.OrderByDescending(o => o.CreatedAt) : q.OrderBy(o => o.CreatedAt)
            };

            return await GetPagedAsync(pageNumber, pageSize, predicate: predicate, orderBy: orderBy, ct: ct);
        }

        public async Task<Order?> GetOrderDetailsAsync(int id, CancellationToken ct = default)
        {
            return await _dbset
                .Include(o => o.Customer)
                .Include(o => o.Cashier)
                .Include(o => o.Branch)
                .Include(o => o.Invoice)
                .FirstOrDefaultAsync(o => o.Id == id, ct);
        }



        public async Task<int> GetTotalOrdersCountAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().AsQueryable();
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.CountAsync(ct);
        }

        public async Task<int> GetCompletedOrdersCountAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.CountAsync(ct);
        }

        public async Task<int> GetPendingOrdersCountAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().Where(o => o.Status == OrderStatus.Pending);
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.CountAsync(ct);
        }

        public async Task<decimal> GetTotalRevenueAsync(int? branchId, CancellationToken ct = default)
        {
            var query = _dbset.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);
            return await query.SumAsync(o => o.TotalAmount, ct);
        }

        public async Task<Order?> GetOrderViaInvoiceNumber(string InvoiceNumber)
        {
            return await _dbset.FirstOrDefaultAsync(o => o.InvoiceNumber == InvoiceNumber);
        }
        // -----------------------------------------------------
        //Dashboardmethods


        // ═══════════════════════════════════════════════════════════════
        // 1. TOTAL ORDERS (Website + MobileApp) — Completed Orders
        //    Returns: Count + Total Revenue for Website & MobileApp
        // ═══════════════════════════════════════════════════════════════
        public async Task<(int CompletedWebsiteOrders, decimal WebsiteRevenue)>
        GetCompletedOnlineOrdersAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed)
                .Where(o => o.Source == OrderSource.Online);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            var grouped = await query
                .GroupBy(o => o.Source)
                .Select(g => new
                {
                    Source = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToListAsync(ct);

            var website = grouped.FirstOrDefault(g => g.Source == OrderSource.Online);

            return (
                website?.Count ?? 0, website?.Revenue ?? 0

            );
        }
        // ═══════════════════════════════════════════════════════════════
        // 2. TOTAL SALES (POS) — POS Orders Count + POS Revenue
        // ═══════════════════════════════════════════════════════════════
        public async Task<(int PosOrdersCount, decimal PosRevenue)> GetPosSalesAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking()
                .Where(o => o.Source == OrderSource.Pos && o.Status == OrderStatus.Completed);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            var posOrdersCount = await query.CountAsync(ct);
            var posRevenue = await query.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            return (posOrdersCount, posRevenue);
        }

        // ═══════════════════════════════════════════════════════════════
        // 3. TOP CUSTOMERS — Based on Order Count + Total Order Value
        //    Returns: CustomerId, FullName, OrderCount, TotalSpent
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<(int CustomerId, string FullName, int OrderCount, decimal TotalSpent)>>
            GetTopCustomersAsync(int topCount, int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking()
                .Where(o => o.CustomerId != null);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            var grouped = await query
                .GroupBy(o => new { o.CustomerId, o.Customer!.FName, o.Customer!.LName })
                .Select(g => new
                {
                    CustomerId = g.Key.CustomerId!.Value,
                    FullName = (g.Key.FName ?? "") + " " + (g.Key.LName ?? ""),
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .OrderByDescending(x => x.OrderCount)
                .ThenByDescending(x => x.TotalSpent)
                .Take(topCount)
                .ToListAsync(ct);

            return grouped.Select(g => (g.CustomerId, g.FullName.Trim(), g.OrderCount, g.TotalSpent)).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // 4. REVENUE BY SOURCE — Completed Orders Only
        //    Returns: POS Revenue, Website Revenue, MobileApp Revenue
        // ═══════════════════════════════════════════════════════════════
        public async Task<(decimal PosRevenue, decimal WebsiteRevenue)>
        GetRevenueBySourceAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed);

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            var grouped = await query
                .GroupBy(o => o.Source)
                .Select(g => new
                {
                    Source = g.Key,
                    Revenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToListAsync(ct);

            var pos = grouped.FirstOrDefault(g => g.Source == OrderSource.Pos);
            var website = grouped.FirstOrDefault(g => g.Source == OrderSource.Online);

            return (
                pos?.Revenue ?? 0,
                website?.Revenue ?? 0

            );
        }

        // ═══════════════════════════════════════════════════════════════
        // 5. TOTAL DISCOUNT REVENUE — Sum of all DiscountAmount
        // ═══════════════════════════════════════════════════════════════
        public async Task<decimal> GetTotalDiscountRevenueAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking();

            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            return await query.SumAsync(o => (decimal?)o.DiscountAmount, ct) ?? 0;
        }

        // ═══════════════════════════════════════════════════════════════
        // 6. TOP BRANCHES — Order Count + Total Revenue per Branch
        //    Returns: BranchId, BranchName, OrderCount, TotalRevenue
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<(int BranchId, string BranchName, int OrderCount, decimal TotalRevenue)>>
            GetTopBranchesAsync(int topCount, CancellationToken ct = default)
        {
            var grouped = await _dbset.AsNoTracking()
                .GroupBy(o => new { o.BranchId, o.Branch!.Name })
                .Select(g => new
                {
                    BranchId = g.Key.BranchId,
                    BranchName = g.Key.Name,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ThenByDescending(x => x.OrderCount)
                .Take(topCount)
                .ToListAsync(ct);

            return grouped.Select(g => (g.BranchId, g.BranchName, g.OrderCount, g.TotalRevenue)).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // 7. SALES OVERVIEW — Updated with all new metrics
        //    Returns: Current/Previous Month Sales, Orders, Revenue
        //           + Today/Yesterday Orders
        //           + Completed Online (Web+App) Orders & Revenue
        //           + POS Sales Count & Revenue
        //           + Total Discount Revenue
        // ═══════════════════════════════════════════════════════════════
        public async Task<(
          int CurrentMonthSales, int PreviousMonthSales,
          int CurrentMonthOrders, int PreviousMonthOrders,
          decimal CurrentMonthRevenue, decimal PreviousMonthRevenue,
          int TodayOrders, int YesterdayOrders,
          int CompletedWebOrders, decimal WebRevenue,
          int PosOrdersCount, decimal PosRevenue,
          decimal TotalDiscountRevenue
      )> GetSalesOverviewRawAsync(int? branchId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var today = now.Date;
            var yesterday = today.AddDays(-1);

            IQueryable<Order> baseQuery = _dbset.AsNoTracking();
            if (branchId.HasValue)
                baseQuery = baseQuery.Where(o => o.BranchId == branchId.Value);

            var currentMonthOrders = baseQuery.Where(o => o.CreatedAt >= currentMonthStart && o.CreatedAt < currentMonthStart.AddMonths(1));
            var previousMonthOrders = baseQuery.Where(o => o.CreatedAt >= previousMonthStart && o.CreatedAt < currentMonthStart);

            // ── TOTAL SALES = Count of Completed POS orders ──
            var currentMonthPosCompleted = currentMonthOrders.Where(o => o.Source == OrderSource.Pos && o.Status == OrderStatus.Completed);
            var previousMonthPosCompleted = previousMonthOrders.Where(o => o.Source == OrderSource.Pos && o.Status == OrderStatus.Completed);
            var currentMonthSales = await currentMonthPosCompleted.CountAsync(ct);
            var previousMonthSales = await previousMonthPosCompleted.CountAsync(ct);

            // ── TOTAL ORDERS = Completed orders from Website + MobileApp ──
            var currentMonthOnlineCompleted = currentMonthOrders
                .Where(o => o.Status == OrderStatus.Completed)
                .Where(o => o.Source == OrderSource.Online);
            var previousMonthOnlineCompleted = previousMonthOrders
                .Where(o => o.Status == OrderStatus.Completed)
                .Where(o => o.Source == OrderSource.Online);
            var currentMonthOrdersCount = await currentMonthOnlineCompleted.CountAsync(ct);
            var previousMonthOrdersCount = await previousMonthOnlineCompleted.CountAsync(ct);

            // ── REVENUE = Completed orders (all sources) ──
            var currentMonthCompleted = currentMonthOrders.Where(o => o.Status == OrderStatus.Completed);
            var previousMonthCompleted = previousMonthOrders.Where(o => o.Status == OrderStatus.Completed);
            var currentMonthRevenue = await currentMonthCompleted.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;
            var previousMonthRevenue = await previousMonthCompleted.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            // ── Day stats (ALL orders today, any source, any status) ──
            var todayOrders = await baseQuery.CountAsync(o => o.CreatedAt >= today && o.CreatedAt < today.AddDays(1), ct);
            var yesterdayOrders = await baseQuery.CountAsync(o => o.CreatedAt >= yesterday && o.CreatedAt < today, ct);

            // ── Completed Web Orders (Website + Completed) ──
            var currentMonthWebCompleted = currentMonthOrders
                .Where(o => o.Status == OrderStatus.Completed && o.Source == OrderSource.Online);
            var completedWebOrders = await currentMonthWebCompleted.CountAsync(ct);
            var webRevenue = await currentMonthWebCompleted.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            // ── POS Stats (Completed POS orders) ──
            var posOrdersCount = await currentMonthPosCompleted.CountAsync(ct);
            var posRevenue = await currentMonthPosCompleted.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;

            // ── Discount Revenue (Completed orders only) ──
            var totalDiscountRevenue = await currentMonthCompleted.SumAsync(o => (decimal?)o.DiscountAmount, ct) ?? 0;

            return (
                currentMonthSales, previousMonthSales,
                currentMonthOrdersCount, previousMonthOrdersCount,
                currentMonthRevenue, previousMonthRevenue,
                todayOrders, yesterdayOrders,
                completedWebOrders, webRevenue,
                posOrdersCount, posRevenue,
                totalDiscountRevenue
            );
        }

        // ═══════════════════════════════════════════════════════════════
        // 8. ORDER STATUS DISTRIBUTION — Unchanged (works fine)
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<(OrderStatus Status, int Count, decimal TotalValue)>>
            GetOrderStatusDistributionAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking();
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            var grouped = await query
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToListAsync(ct);

            return grouped.Select(g => (g.Status, g.Count, g.TotalValue)).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // 9. ORDERS BY SOURCE DISTRIBUTION — Unchanged (works fine)
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<(OrderSource Source, int Count, decimal TotalValue)>>
            GetOrdersBySourceDistributionAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking();
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            var grouped = await query
                .GroupBy(o => o.Source)
                .Select(g => new
                {
                    Source = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => (decimal?)o.TotalAmount) ?? 0
                })
                .ToListAsync(ct);

            return grouped.Select(g => (g.Source, g.Count, g.TotalValue)).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // 10. TOP SELLING PRODUCTS — Unchanged (works fine)
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<(int ProductId, string ProductName, int QuantitySold)>>
            GetTopSellingProductsAsync(int topCount, int? branchId, CancellationToken ct = default)
        {
            IQueryable<OrderItem> query = _context.Set<OrderItem>()
                .AsNoTracking()
                .Where(oi => oi.Order!.Status == OrderStatus.Completed);

            if (branchId.HasValue)
                query = query.Where(oi => oi.Order!.BranchId == branchId.Value);

            var grouped = await query
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    QuantitySold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(topCount)
                .ToListAsync(ct);

            return grouped.Select(g => (g.ProductId, g.ProductName, g.QuantitySold)).ToList();
        }

        // ═══════════════════════════════════════════════════════════════
        // 11. BEST & LEAST SELLING PRODUCT COUNTS — Unchanged
        // ═══════════════════════════════════════════════════════════════
        public async Task<(int BestSellingCount, int LeastSellingCount)>
            GetBestAndLeastSellingProductCountsAsync(int? branchId, CancellationToken ct = default)
        {
            IQueryable<OrderItem> query = _context.Set<OrderItem>()
                .AsNoTracking()
                .Where(oi => oi.Order!.Status == OrderStatus.Completed);

            if (branchId.HasValue)
                query = query.Where(oi => oi.Order!.BranchId == branchId.Value);

            var productSales = await query
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) })
                .ToListAsync(ct);

            if (!productSales.Any())
                return (0, 0);

            var average = productSales.Average(p => p.TotalQuantity);
            var bestCount = productSales.Count(p => p.TotalQuantity >= average);
            var leastCount = productSales.Count(p => p.TotalQuantity < average);

            return (bestCount, leastCount);
        }

        // ═══════════════════════════════════════════════════════════════
        // 12. LATEST ORDERS PAGED — Unchanged (works fine)
        // ═══════════════════════════════════════════════════════════════
        public async Task<(IReadOnlyList<Order> Items, int TotalCount)>
            GetLatestOrdersPagedAsync(int pageNumber, int pageSize, int? branchId, CancellationToken ct = default)
        {
            IQueryable<Order> query = _dbset.AsNoTracking();
            if (branchId.HasValue)
                query = query.Where(o => o.BranchId == branchId.Value);

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}


