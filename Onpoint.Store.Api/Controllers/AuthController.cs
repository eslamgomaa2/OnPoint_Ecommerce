using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.Services.AuthServices;
using Onpoint.Store.Application.Services.AuthServices.ExternalAuthService;

namespace Onpoint.Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IExternalAuthService _externalAuthService;

        public AuthController(IAuthService authService, IExternalAuthService externalAuthService)
        {
            _authService = authService;
            _externalAuthService = externalAuthService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct = default)
        {
            var result = await _authService.RegisterAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto, CancellationToken ct = default)
        {
            var result = await _authService.VerifyOtpAsync(dto.Email, dto.Token);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto, CancellationToken ct = default)
        {
            var result = await _externalAuthService.ExternalLoginAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct = default)
        {
            var result = await _authService.LoginAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto, CancellationToken ct = default)
        {
            var result = await _authService.ResendOtpAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct = default)
        {
            var result = await _authService.ForgotPasswordAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct = default)
        {
            var result = await _authService.ResetPasswordAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}