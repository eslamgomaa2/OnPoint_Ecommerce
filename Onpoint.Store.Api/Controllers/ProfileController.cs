using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.Services.Profile;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileServices _profileServices;
    public ProfileController(IProfileServices profileServices)
    {
        _profileServices = profileServices;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
        return userId;
    }


    [HttpDelete]
    public async Task<IActionResult> DeleteMyAccount(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var result = await _profileServices.DeleteMyAccountAsync(userId);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}