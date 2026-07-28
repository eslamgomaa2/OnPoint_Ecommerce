using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review, int>
    {
        Task<IReadOnlyList<Review>> GetAllWithDetailsAsync(CancellationToken ct = default);
        Task<List<Review>> GetProductReviewsAsync(int productId, bool onlyApproved, CancellationToken ct = default);
        Task<List<Review>> GetUserReviewsAsync(int userId, CancellationToken ct = default);
        Task<Review?> GetByIdForUserAsync(int reviewId, int userId, CancellationToken ct = default);
    }
}