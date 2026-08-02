
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IInvoiceRepo : IGenericRepository<Order, int>
    {
        Task<(IReadOnlyList<Order> Items, int TotalCount)> GetInvoicesPagedAsync(
    int pageNumber,
    int pageSize,
    string? search,
    DateTime? dateFrom,
    DateTime? dateTo,
    int? branchId,
    OrderSource? orderSource,
    CancellationToken ct = default);

        Task<Order?> GetInvoiceDetailsAsync(int id, CancellationToken ct = default);
        Task<(int Total, int Paid, int Pending, int Overdue)> GetInvoiceStatisticsAsync(
    int? branchId,
    CancellationToken ct = default);
    }
}