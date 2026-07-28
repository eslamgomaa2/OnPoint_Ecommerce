
using Onpoint.Store.Domin.Entities.Identity;

namespace Onpoint.Store.Domin.Repositories
{
    public interface ICustomerRepository : IGenericRepository<Customer, int>
    {
        Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
                int? branchId,
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<Customer?> GetByIdWithOrdersAsync(int id, int? branchId, CancellationToken ct = default);
        Task<Customer?> GetByPhoneNumberAsync(string phone, int? branchId, CancellationToken ct = default);
        Task<(int TotalCustomers, int Active, int NewThisMonth)> GetCustomerCountsAsync(
            int? branchId,
            CancellationToken ct = default);
    }
}