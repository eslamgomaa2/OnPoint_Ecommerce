using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

public class AccountingRepository : IAccountingRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Order> _orders;
    private readonly DbSet<Branch> _branches;

    public AccountingRepository(ApplicationDbContext context)
    {
        _context = context;
        _orders = context.Set<Order>();
        _branches = context.Set<Branch>();
    }

    public async Task<int> GetTotalBranchesCountAsync(CancellationToken ct)
    {
        return await _branches.AsNoTracking().CountAsync(ct);
    }

    public async Task<(int Count, decimal Revenue)> GetSalesMetricsAsync(int? branchId, CancellationToken ct)
    {
        var query = _orders.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
        if (branchId.HasValue) query = query.Where(o => o.BranchId == branchId.Value);

        var count = await query.CountAsync(ct);
        var revenue = await query.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;
        return (count, revenue);
    }

    public async Task<decimal> GetTotalEarningsAsync(int? branchId, CancellationToken ct)
    {
        var query = _orders.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
        if (branchId.HasValue) query = query.Where(o => o.BranchId == branchId.Value);

        return await query.SumAsync(o => (decimal?)o.SubTotal, ct) ?? 0;
    }

    public async Task<decimal> GetTotalRevenueAsync(int? branchId, CancellationToken ct)
    {
        var query = _orders.AsNoTracking().Where(o => o.Status == OrderStatus.Completed);
        if (branchId.HasValue) query = query.Where(o => o.BranchId == branchId.Value);

        return await query.SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0;
    }

    public async Task<IEnumerable<(string Label, decimal Value)>> GetDailyReportsAsync(int? branchId, CancellationToken ct)
    {
        var query = _orders.AsNoTracking().Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-10));
        if (branchId.HasValue) query = query.Where(o => o.BranchId == branchId.Value);

        var rawData = await query
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync(ct);

        return rawData.Select(x => (x.Date.ToString("dd MMM"), x.Total));
    }

    public async Task<IEnumerable<(int Month, decimal Value)>> GetMonthlyReportsAsync(int? branchId, CancellationToken ct)
    {
        var query = _orders.AsNoTracking().Where(o => o.CreatedAt.Year == DateTime.UtcNow.Year);
        if (branchId.HasValue) query = query.Where(o => o.BranchId == branchId.Value);

        var rawData = await query
            .GroupBy(o => o.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync(ct);

        return rawData.Select(x => (x.Month, x.Total));
    }

    public async Task<IEnumerable<(int Year, decimal Value)>> GetYearlyReportsAsync(int? branchId, CancellationToken ct)
    {
        var query = _orders.AsNoTracking();
        if (branchId.HasValue) query = query.Where(o => o.BranchId == branchId.Value);

        var rawData = await query
            .GroupBy(o => o.CreatedAt.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(o => o.TotalAmount) })
            .ToListAsync(ct);

        return rawData.Select(x => (x.Year, x.Total));
    }
}