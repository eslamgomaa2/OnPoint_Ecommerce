using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class AddressRepository : GenericRepository<Address, int>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Address>> GetUserAddressesAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Address?> GetUserDefaultAddressAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault, ct);
        }
    }
}
