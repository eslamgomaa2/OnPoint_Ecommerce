namespace Onpoint.Store.Domin.Repositories
{
    using global::Onpoint.Store.Domin.Entities;

    namespace Onpoint.Store.Application.Interfaces.Repositories
    {
        public interface IRefreshTokenRepository : IGenericRepository<RefreshToken, int>
        {

            Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
            Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default);
            Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(int userId, CancellationToken ct = default);
            Task<int> CountActiveTokensByUserIdAsync(int userId, CancellationToken ct = default);

            Task RevokeAsync(string token, CancellationToken ct = default);
            Task RevokeAllByUserIdAsync(int userId, CancellationToken ct = default);

            // Cleanup
            Task DeleteExpiredAndRevokedAsync(int userId, CancellationToken ct = default);
            Task<int> DeleteAllExpiredAndRevokedAsync(CancellationToken ct = default);

            // Save
            Task SaveChangesAsync(CancellationToken ct = default);
        }
    }
}
