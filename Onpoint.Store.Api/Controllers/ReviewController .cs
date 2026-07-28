using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Review;
using Onpoint.Store.Application.Services.ReviewServ;
using System.Security.Claims;

[Route("api/reviews")]
[ApiController]
[Authorize(Roles = "Customer" + "," + "SuperAdmin")]
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

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _reviewService.CreateReviewAsync(userId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{reviewId}")]
    public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto dto, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _reviewService.UpdateReviewAsync(userId, reviewId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(int reviewId, CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _reviewService.DeleteReviewAsync(userId, reviewId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    // GET /api/reviews/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyReviews(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _reviewService.GetMyReviewsAsync(userId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}