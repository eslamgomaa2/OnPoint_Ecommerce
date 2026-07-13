using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IAddressRepository : IGenericRepository<Address, int>
    {
        Task<IReadOnlyList<Address>> GetUserAddressesAsync(int userId, CancellationToken ct = default);
        Task<Address?> GetUserDefaultAddressAsync(int userId, CancellationToken ct = default);
    }
}
