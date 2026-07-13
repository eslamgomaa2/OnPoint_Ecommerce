using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Review;

namespace Onpoint.Store.Application.Services.ReviewServ
{
    public interface IReviewService
    {
        Task<ServiceResult<ReviewDto>> CreateReviewAsync(int userId, CreateReviewDto dto, CancellationToken ct = default);
        Task<ServiceResult<ReviewDto>> UpdateReviewAsync(int userId, UpdateReviewDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteReviewAsync(int userId, int reviewId, CancellationToken ct = default);
        Task<ServiceResult<List<ReviewDto>>> GetProductReviewsAsync(int productId, CancellationToken ct = default);
        Task<ServiceResult<List<ReviewDto>>> GetMyReviewsAsync(int userId, CancellationToken ct = default);


        Task<ServiceResult<string>> ApproveReviewAsync(int reviewId, CancellationToken ct = default);
        Task<ServiceResult<string>> RejectReviewAsync(int reviewId, CancellationToken ct = default);
        Task<ServiceResult<List<ReviewDto>>> GetPendingReviewsAsync(CancellationToken ct = default);
    }
}
