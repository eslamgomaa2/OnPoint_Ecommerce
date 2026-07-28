
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities.Identity;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer, int>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(int? branchId, string? search, bool? isActive, int pageNumber, int pageSize,
     CancellationToken ct = default)
        {
            IQueryable<Customer> query = _dbset
                .AsNoTracking()
                .Include(c => c.Orders)
                .Where(c => !c.IsDeleted);


            if (branchId.HasValue)
                query = query.Where(c => c.BranchId == branchId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    (c.FName != null && c.FName.ToLower().Contains(term)) ||
                    (c.LName != null && c.LName.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.Phone != null && c.Phone.Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<Customer?> GetByIdWithOrdersAsync(int id, int? branchId, CancellationToken ct = default)
        {
            IQueryable<Customer> query = _dbset
                .AsNoTracking()
                .Include(c => c.Orders)
                .Where(c => c.Id == id && !c.IsDeleted);


            if (branchId.HasValue)
                query = query.Where(c => c.BranchId == branchId.Value);

            return await query.FirstOrDefaultAsync(ct);
        }
        public async Task<(int TotalCustomers, int Active, int NewThisMonth)> GetCustomerCountsAsync(int? branchId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            IQueryable<Customer> baseQuery = _dbset
                .AsNoTracking()
                .Where(c => !c.IsDeleted);


            if (branchId.HasValue)
                baseQuery = baseQuery.Where(c => c.BranchId == branchId.Value);

            var totalCustomers = await baseQuery.CountAsync(ct);
            var active = await baseQuery.CountAsync(c => c.IsActive, ct);
            var newThisMonth = await baseQuery.CountAsync(
                c => c.CreatedAt.Year == now.Year && c.CreatedAt.Month == now.Month, ct);

            return (totalCustomers, active, newThisMonth);
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phone, int? branchId, CancellationToken ct = default)
        {
            IQueryable<Customer> query = _dbset
                .AsNoTracking()
                .Where(c => c.Phone == phone && !c.IsDeleted);


            if (branchId.HasValue)
                query = query.Where(c => c.BranchId == branchId.Value);

            return await query.FirstOrDefaultAsync(ct);
        }
    }
}