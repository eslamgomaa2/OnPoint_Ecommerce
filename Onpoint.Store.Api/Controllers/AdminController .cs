using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.Services.ReviewServ;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public AdminController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }



        [HttpPut("admin/{reviewId}/approve")]

        public async Task<IActionResult> ApproveReview(int reviewId, CancellationToken ct = default)
        {
            var result = await _reviewService.ApproveReviewAsync(reviewId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("admin/{reviewId}/reject")]

        public async Task<IActionResult> RejectReview(int reviewId, CancellationToken ct = default)
        {
            var result = await _reviewService.RejectReviewAsync(reviewId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("admin/pending")]

        public async Task<IActionResult> GetPendingReviews(CancellationToken ct = default)
        {
            var result = await _reviewService.GetPendingReviewsAsync(ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}