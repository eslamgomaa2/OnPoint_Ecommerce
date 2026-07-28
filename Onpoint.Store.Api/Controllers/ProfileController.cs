using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.DTOs.Auth.Profile;
using Onpoint.Store.Application.Services.Profile;
using System.Security.Claims;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileServices _profileServices;

    public ProfileController(IProfileServices profileServices)
    {
        _profileServices = profileServices;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ServiceResult<UserLoginInfoDto>>> GetMyInfo(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _profileServices.GetMyLoginInfoAsync(userId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ServiceResult<UpdateMyProfileDto>>> UpdateMyProfile(
        [FromBody] UpdateMyProfileDto dto,
        CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _profileServices.UpdateMyAccount(userId, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("me")]
    public async Task<ActionResult<ServiceResult<bool>>> DeleteMyAccount(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _profileServices.DeleteMyAccountAsync(userId);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}