using AutoMapper;
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Common;
using Onpoint.Store.Application.DTOs.DashBoard;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper, ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
        }

        // ═══════════════════════════════════════════════════════════════
        // 1. SALES OVERVIEW — ALL TotalAmount = Completed orders only
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<SalesOverviewDto>> GetSalesOverviewAsync(int? branchId, CancellationToken ct = default)
        {
            var (currentMonthSales, previousMonthSales,
                 currentMonthOrders, previousMonthOrders,
                 currentMonthRevenue, previousMonthRevenue,
                 todayOrders, yesterdayOrders,
                 completedWebOrders, webRevenue,
                 posOrdersCount, posRevenue,
                 totalDiscountRevenue) = await _unitOfWork.Orders.GetSalesOverviewRawAsync(branchId, ct);

            var dto = new SalesOverviewDto
            {
                // If TotalSales is now count of POS orders, you can attach posRevenue here:
                TotalSales = BuildMetric(currentMonthSales, previousMonthSales, posRevenue),

                // If TotalOrders is online orders, you can attach webRevenue here:
                TotalOrders = BuildMetric(currentMonthOrders, previousMonthOrders, webRevenue),

                // Overall Revenue
                Revenue = BuildMetric(currentMonthRevenue, previousMonthRevenue, currentMonthRevenue),

                // Daily Orders (if you track daily revenue, you can pass it here, otherwise 0)
                DailyOrders = BuildMetric(todayOrders, yesterdayOrders, 0),

                CompletedWebOrders = new MetricDto { Value = completedWebOrders, Revenue = webRevenue, PercentageChange = 0, IsIncrease = true },
                PosSales = new MetricDto { Value = posOrdersCount, Revenue = posRevenue, PercentageChange = 0, IsIncrease = true },
                TotalDiscountRevenue = new MetricDto { Value = totalDiscountRevenue, Revenue = 0, PercentageChange = 0, IsIncrease = true }
            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 2. ORDER STATUS DISTRIBUTION
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<OrderStatusDistributionDto>> GetOrderStatusDistributionAsync(int? branchId, CancellationToken ct = default)
        {
            var groups = await _unitOfWork.Orders.GetOrderStatusDistributionAsync(branchId, ct);

            var totalOrders = groups.Sum(g => g.Count);
            var totalValue = groups.Sum(g => g.TotalValue);

            var dto = new OrderStatusDistributionDto
            {
                TotalOrders = totalOrders,
                TotalValue = totalValue,
                Breakdown = groups.Select(g => new OrderStatusBreakdownDto
                {
                    Status = g.Status,
                    StatusName = g.Status.ToString(),
                    Count = g.Count,
                    TotalValue = g.TotalValue,
                    Percentage = totalOrders == 0 ? 0 : Math.Round((decimal)g.Count / totalOrders * 100, 2)
                }).ToList()
            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 3. DASHBOARD HIGHLIGHTS
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<DashboardHighlightsDto>> GetHighlightsAsync(int? branchId, CancellationToken ct = default)
        {
            var (bestCount, leastCount) = await _unitOfWork.Orders.GetBestAndLeastSellingProductCountsAsync(branchId, ct);

            var topCustomers = await _unitOfWork.Orders.GetTopCustomersAsync(1000, branchId, ct);
            var topBranches = await _unitOfWork.Orders.GetTopBranchesAsync(1000, ct);

            var inventoryCount = await _unitOfWork.Products.CountAsync();

            var dto = new DashboardHighlightsDto
            {
                BestSellingProductsCount = bestCount,
                LeastSellingProductsCount = leastCount,
                TopCustomersCount = topCustomers.Count,
                TopBranchesCount = topBranches.Count,
                InventoryCount = inventoryCount
            };

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<List<TopCustomerDto>>> GetTopCustomersAsync(int count, int? branchId, CancellationToken ct = default)
        {
            var customers = await _unitOfWork.Orders.GetTopCustomersAsync(count, branchId, ct);

            var dto = customers.Select(c => new TopCustomerDto
            {
                CustomerId = c.CustomerId,
                FullName = c.FullName,
                OrderCount = c.OrderCount,
                TotalSpent = c.TotalSpent
            }).ToList();

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<List<TopBranchDto>>> GetTopBranchesAsync(int count, CancellationToken ct = default)
        {
            var branches = await _unitOfWork.Orders.GetTopBranchesAsync(count, ct);

            var dto = branches.Select(b => new TopBranchDto
            {
                BranchId = b.BranchId,
                BranchName = b.BranchName,
                OrderCount = b.OrderCount,
                TotalRevenue = b.TotalRevenue
            }).ToList();

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 4. LATEST ORDERS PAGED
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<PagedResultDto<LatestOrderDto>>> GetLatestOrdersAsync(int pageNumber, int pageSize, int? branchId, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Orders.GetLatestOrdersPagedAsync(pageNumber, pageSize, branchId, ct);

            var dto = new PagedResultDto<LatestOrderDto>
            {
                Items = _mapper.Map<List<LatestOrderDto>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 5. ORDERS BY SOURCE
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<OrdersBySourceDto>> GetOrdersBySourceAsync(int? branchId, CancellationToken ct = default)
        {
            var groups = await _unitOfWork.Orders.GetOrdersBySourceDistributionAsync(branchId, ct);
            var totalOrders = groups.Sum(g => g.Count);
            var totalValue = groups.Sum(g => g.TotalValue);

            var dto = new OrdersBySourceDto
            {
                TotalOrders = totalOrders,
                TotalValue = totalValue,
                Breakdown = groups.Select(g => new OrderSourceBreakdownDto
                {
                    Source = g.Source,
                    SourceName = g.Source.ToString(),
                    Count = g.Count,
                    TotalValue = g.TotalValue,
                    Percentage = totalOrders == 0 ? 0 : Math.Round((decimal)g.Count / totalOrders * 100, 2)
                }).ToList()
            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 6. REVENUE BY SOURCE (Completed only)
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<RevenueBySourceDto>> GetRevenueBySourceAsync(int? branchId, CancellationToken ct = default)
        {
            var (posRevenue, websiteRevenue) = await _unitOfWork.Orders.GetRevenueBySourceAsync(branchId, ct);

            var dto = new RevenueBySourceDto
            {
                PosRevenue = posRevenue,
                WebsiteRevenue = websiteRevenue,

            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 7. TOP SELLING PRODUCTS
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int count, int? branchId, CancellationToken ct = default)
        {
            var products = await _unitOfWork.Orders.GetTopSellingProductsAsync(count, branchId, ct);

            var dto = products.Select(p => new TopSellingProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                QuantitySold = p.QuantitySold
            }).ToList();

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 8. POS SALES (Completed only)
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<MetricDto>> GetPosSalesAsync(int? branchId, CancellationToken ct = default)
        {
            var (posOrdersCount, posRevenue) = await _unitOfWork.Orders.GetPosSalesAsync(branchId, ct);

            var dto = new MetricDto
            {
                Value = posOrdersCount,
                Revenue = posRevenue,
                PercentageChange = 0,
                IsIncrease = true
            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // 9. COMPLETED ONLINE ORDERS
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<MetricDto>> GetCompletedOnlineOrdersAsync(int? branchId, CancellationToken ct = default)
        {
            var (webCount, webRev) = await _unitOfWork.Orders.GetCompletedOnlineOrdersAsync(branchId, ct);

            var webMetric = new MetricDto
            {
                Value = webCount,
                Revenue = webRev,
                PercentageChange = 0,
                IsIncrease = true
            };

            return _resultHandler.Success(webMetric);
        }

        // ═══════════════════════════════════════════════════════════════
        // 10. TOTAL DISCOUNT REVENUE (Completed only)
        // ═══════════════════════════════════════════════════════════════
        public async Task<ServiceResult<MetricDto>> GetTotalDiscountRevenueAsync(int? branchId, CancellationToken ct = default)
        {
            var totalDiscount = await _unitOfWork.Orders.GetTotalDiscountRevenueAsync(branchId, ct);

            var dto = new MetricDto
            {
                Value = totalDiscount,
                Revenue = totalDiscount,
                PercentageChange = 0,
                IsIncrease = true
            };

            return _resultHandler.Success(dto);
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════════
        private MetricDto BuildMetric(decimal currentValue, decimal previousValue, decimal currentRevenue = 0)
        {
            // Calculate percentage change based on Value
            decimal percentageChange = 0;
            bool isIncrease = true;

            if (previousValue > 0)
            {
                percentageChange = ((currentValue - previousValue) / previousValue) * 100;
                isIncrease = percentageChange >= 0;
                percentageChange = Math.Abs(percentageChange);
            }
            else if (currentValue > 0)
            {
                percentageChange = 100;
                isIncrease = true;
            }

            return new MetricDto
            {
                Value = currentValue,
                Revenue = currentRevenue,
                PercentageChange = Math.Round(percentageChange, 2),
                IsIncrease = isIncrease
            };
        }

        private static decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0)
                return current == 0 ? 0 : 100;

            return Math.Round((current - previous) / previous * 100, 2);
        }
    }
}