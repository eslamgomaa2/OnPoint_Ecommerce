using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using global::Onpoint.Store.Application.DTOs.Review;
using global::Onpoint.Store.Domin.Entities;
using global::Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.ReviewServ
{


    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IValidator<CreateReviewDto> _createValidator;
        private readonly IValidator<UpdateReviewDto> _updateValidator;

        public ReviewService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ServiceResultHandler resultHandler,
            IValidator<CreateReviewDto> createValidator,
            IValidator<UpdateReviewDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _resultHandler = resultHandler;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<ServiceResult<ReviewDto>> CreateReviewAsync(int userId, CreateReviewDto dto, CancellationToken ct = default)
        {
            var validation = await _createValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId, ct);
            if (product == null || product.IsDeleted)
                return _resultHandler.NotFound<ReviewDto>("Product not found.");


            var hasReceived = await _unitOfWork.Orders.HasUserReceivedProductAsync(userId, dto.ProductId, ct);
            if (!hasReceived)
                return _resultHandler.BadRequest<ReviewDto>(
                    "You can only review products you have purchased and received.");

            var review = new Review
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsApproved = false
            };

            await _unitOfWork.Reviews.AddAsync(review, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var saved = await _unitOfWork.Reviews.GetByIdForUserAsync(review.Id, userId, ct);
            return _resultHandler.Created(_mapper.Map<ReviewDto>(saved));
        }

        public async Task<ServiceResult<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto, CancellationToken ct = default)
        {
            var validation = await _updateValidator.ValidateAsync(dto, ct);
            if (!validation.IsValid) throw new ValidationException(validation.Errors);

            var review = await _unitOfWork.Reviews.GetByIdForUserAsync(reviewId, userId, ct);
            if (review == null)
                return _resultHandler.NotFound<ReviewDto>("Review not found.");


            if (review.IsApproved)
                return _resultHandler.BadRequest<ReviewDto>("Cannot edit a review that has already been approved.");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<ReviewDto>(review));
        }

        public async Task<ServiceResult<string>> DeleteReviewAsync(int userId, int reviewId, CancellationToken ct = default)
        {
            var review = await _unitOfWork.Reviews.GetByIdForUserAsync(reviewId, userId, ct);
            if (review == null)
                return _resultHandler.NotFound<string>("Review not found.");

            if (review.IsApproved)
                return _resultHandler.BadRequest<string>("Cannot delete a review that has already been approved.");

            _unitOfWork.Reviews.Remove(review);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>("Review deleted.");
        }

        public async Task<ServiceResult<List<ReviewDto>>> GetProductReviewsAsync(int productId, CancellationToken ct = default)
        {

            var reviews = await _unitOfWork.Reviews.GetProductReviewsAsync(productId, onlyApproved: true, ct);
            return _resultHandler.Success(_mapper.Map<List<ReviewDto>>(reviews));
        }

        public async Task<ServiceResult<List<ReviewDto>>> GetMyReviewsAsync(int userId, CancellationToken ct = default)
        {

            var reviews = await _unitOfWork.Reviews.GetUserReviewsAsync(userId, ct);
            return _resultHandler.Success(_mapper.Map<List<ReviewDto>>(reviews));
        }

        // ============ Admin ============

        public async Task<ServiceResult<string>> ApproveReviewAsync(int reviewId, CancellationToken ct = default)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId, ct);
            if (review == null)
                return _resultHandler.NotFound<string>("Review not found.");

            review.IsApproved = true;
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success<string>("Review approved.");
        }

        public async Task<ServiceResult<string>> RejectReviewAsync(int reviewId, CancellationToken ct = default)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId, ct);
            if (review == null)
                return _resultHandler.NotFound<string>("Review not found.");


            _unitOfWork.Reviews.Remove(review);
            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Deleted<string>("Review rejected and removed.");
        }

        public async Task<ServiceResult<List<ReviewDto>>> GetPendingReviewsAsync(CancellationToken ct = default)
        {
            var reviews = await _unitOfWork.Reviews.GetAllWithDetailsAsync(ct);
            var pending = reviews.Where(r => !r.IsApproved).ToList();
            return _resultHandler.Success(_mapper.Map<List<ReviewDto>>(pending));
        }
    }
}

