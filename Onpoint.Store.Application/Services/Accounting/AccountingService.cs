using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.DashBoard;
using Onpoint.Store.Application.DTOs.DashBoard.Accounting;
using Onpoint.Store.Application.Services.Accounting;
using Onpoint.Store.Domin.Repositories;
using System.Globalization;

public class AccountingService : IAccountingService
{
    private readonly IAccountingRepository _accountingRepo;
    private readonly ServiceResultHandler _resultHandler;

    public AccountingService(IAccountingRepository accountingRepo, ServiceResultHandler resultHandler)
    {
        _accountingRepo = accountingRepo;
        _resultHandler = resultHandler;
    }

    public async Task<ServiceResult<AccountingDashboardDto>> GetAccountingDashboardAsync(int? branchId, CancellationToken ct)
    {
        var branchesCount = await _accountingRepo.GetTotalBranchesCountAsync(ct);
        var salesMetric = await _accountingRepo.GetSalesMetricsAsync(branchId, ct);
        var earnings = await _accountingRepo.GetTotalEarningsAsync(branchId, ct);
        var revenue = await _accountingRepo.GetTotalRevenueAsync(branchId, ct);

        // استقبال Tuples
        var dailyTuples = await _accountingRepo.GetDailyReportsAsync(branchId, ct);
        var monthlyTuples = await _accountingRepo.GetMonthlyReportsAsync(branchId, ct);
        var yearlyTuples = await _accountingRepo.GetYearlyReportsAsync(branchId, ct);

        // عمل Mapping داخل الـ Service لتصبح DTOs
        var dailyReports = dailyTuples.Select(d => new ChartDataPointDto { Label = d.Label, Value = d.Value }).ToList();

        var monthlyReports = monthlyTuples.Select(m => new MonthlyBarChartDto
        {
            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(m.Month),
            Value = m.Value
        }).ToList();

        var yearlyReports = yearlyTuples.Select(y => new ChartDataPointDto
        {
            Label = y.Year.ToString(),
            Value = y.Value
        }).ToList();

        var dto = new AccountingDashboardDto
        {
            BranchReport = new MetricDto { Value = branchesCount, PercentageChange = 0, IsIncrease = true },
            SalesReport = new MetricDto { Value = salesMetric.Count, Revenue = salesMetric.Revenue, PercentageChange = 8.4m, IsIncrease = true },
            EarningsReports = new MetricDto { Value = earnings, Revenue = earnings, PercentageChange = 8.4m, IsIncrease = true },
            RevenueReport = new MetricDto { Value = revenue, Revenue = revenue, PercentageChange = 8.4m, IsIncrease = true },

            DailyReports = dailyReports,
            MonthlyReports = monthlyReports,
            YearlyReports = yearlyReports,

            DonutReport = new DonutChartDto
            {
                TotalValue = revenue,
                Slices = new List<DonutSliceDto>
                {
                    new() { Category = "Revenue", Value = revenue * 0.3m, Percentage = 30 },
                    new() { Category = "Earnings", Value = earnings * 0.3m, Percentage = 30 },
                    new() { Category = "Expenses", Value = revenue * 0.3m, Percentage = 30 },
                    new() { Category = "Taxes", Value = revenue * 0.1m, Percentage = 10 }
                }
            }
        };

        return _resultHandler.Success(dto);
    }
}