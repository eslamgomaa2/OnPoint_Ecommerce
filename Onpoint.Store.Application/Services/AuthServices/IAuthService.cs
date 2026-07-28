using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Services.AuthServices
{
    public interface IAuthService
    {
        Task<ServiceResult<string>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> ConfirmEmailAsync(ConfirmEmailDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> ResendOtpAsync(ResendOtpDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> VerifyOtpAsync(string email, string otpCode);
        Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<ServiceResult<string>> RevokeTokenAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<string>> LogoutAsync(int userId, CancellationToken ct = default);
    }
}