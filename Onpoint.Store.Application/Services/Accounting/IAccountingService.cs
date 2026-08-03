using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.DashBoard.Accounting;

namespace Onpoint.Store.Application.Services.Accounting
{
    public interface IAccountingService
    {
        Task<ServiceResult<AccountingDashboardDto>> GetAccountingDashboardAsync(int? branchId, CancellationToken ct);
    }
}
