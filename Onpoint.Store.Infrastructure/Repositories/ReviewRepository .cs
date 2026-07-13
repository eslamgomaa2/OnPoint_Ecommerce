using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review, int>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Review>> GetProductReviewsAsync(int productId, bool onlyApproved, CancellationToken ct = default)
        {
            var query = _dbset
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId);

            if (onlyApproved)
                query = query.Where(r => r.IsApproved);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Review>> GetUserReviewsAsync(int userId, CancellationToken ct = default)
        {
            return await _dbset
                .Include(r => r.Product).Include(o => o.User)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<Review?> GetByIdForUserAsync(int reviewId, int userId, CancellationToken ct = default)
        {
            return await _dbset.Include(o => o.User).Include(o => o.Product)
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId, ct);
        }
    }
}