using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Review;
using Onpoint.Store.Application.Services.ReviewServ;
using System.Security.Claims;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");

            return userId;
        }

        [HttpPost("[Action]")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _reviewService.CreateReviewAsync(userId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("[Action]")]
        public async Task<IActionResult> UpdateReview([FromBody] UpdateReviewDto dto, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _reviewService.UpdateReviewAsync(userId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _reviewService.DeleteReviewAsync(userId, reviewId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviews(int productId, CancellationToken ct = default)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("my-reviews")]
        public async Task<IActionResult> GetMyReviews(CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _reviewService.GetMyReviewsAsync(userId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


    }
}