using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.Services.ReviewServ;

[Route("api/admin/reviews")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class AdminReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public AdminReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReviews(CancellationToken ct = default)
    {
        var result = await _reviewService.GetPendingReviewsAsync(ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{reviewId}/approve")]
    public async Task<IActionResult> ApproveReview(int reviewId, CancellationToken ct = default)
    {
        var result = await _reviewService.ApproveReviewAsync(reviewId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{reviewId}/reject")]
    public async Task<IActionResult> RejectReview(int reviewId, CancellationToken ct = default)
    {
        var result = await _reviewService.RejectReviewAsync(reviewId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}