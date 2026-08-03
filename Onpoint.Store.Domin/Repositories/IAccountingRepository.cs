namespace Onpoint.Store.Domin.Repositories
{


    public interface IAccountingRepository
    {
        Task<int> GetTotalBranchesCountAsync(CancellationToken ct);
        Task<(int Count, decimal Revenue)> GetSalesMetricsAsync(int? branchId, CancellationToken ct);
        Task<decimal> GetTotalEarningsAsync(int? branchId, CancellationToken ct);
        Task<decimal> GetTotalRevenueAsync(int? branchId, CancellationToken ct);


        Task<IEnumerable<(string Label, decimal Value)>> GetDailyReportsAsync(int? branchId, CancellationToken ct);
        Task<IEnumerable<(int Month, decimal Value)>> GetMonthlyReportsAsync(int? branchId, CancellationToken ct);
        Task<IEnumerable<(int Year, decimal Value)>> GetYearlyReportsAsync(int? branchId, CancellationToken ct);
    }
}

