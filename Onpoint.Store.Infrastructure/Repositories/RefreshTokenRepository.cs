using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories.Onpoint.Store.Application.Interfaces.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken, int>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RefreshTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);
        }

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokens
                .Include(rt => rt.ApplicationUser)
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);
        }

        public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokens
                .AsNoTracking()
                .Where(rt => rt.ApplicationUserId == userId
                          && rt.RevokedAt == null
                          && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(ct);
        }

        public async Task<int> CountActiveTokensByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _dbContext.RefreshTokens
                .CountAsync(rt => rt.ApplicationUserId == userId
                               && rt.RevokedAt == null
                               && rt.ExpiresAt > DateTime.UtcNow, ct);
        }



        public async Task RevokeAsync(string token, CancellationToken ct = default)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);

            if (refreshToken != null && refreshToken.RevokedAt == null)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                _dbContext.RefreshTokens.Update(refreshToken);
            }
        }

        public async Task RevokeAllByUserIdAsync(int userId, CancellationToken ct = default)
        {
            var tokens = await _dbContext.RefreshTokens
                .Where(rt => rt.ApplicationUserId == userId && rt.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task DeleteExpiredAndRevokedAsync(int userId, CancellationToken ct = default)
        {
            var tokensToDelete = await _dbContext.RefreshTokens
                .Where(rt => rt.ApplicationUserId == userId
                          && (rt.RevokedAt != null || rt.ExpiresAt <= DateTime.UtcNow))
                .ToListAsync(ct);

            _dbContext.RefreshTokens.RemoveRange(tokensToDelete);
        }

        public async Task<int> DeleteAllExpiredAndRevokedAsync(CancellationToken ct = default)
        {
            var tokensToDelete = await _dbContext.RefreshTokens
                .Where(rt => rt.RevokedAt != null || rt.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync(ct);

            _dbContext.RefreshTokens.RemoveRange(tokensToDelete);
            return tokensToDelete.Count;
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _dbContext.SaveChangesAsync(ct);
        }
    }
}