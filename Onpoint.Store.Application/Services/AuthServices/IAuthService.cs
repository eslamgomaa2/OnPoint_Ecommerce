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
        Task<ServiceResult<PasswordResetTokenResponseDto>> VerifyResetCodeAsync(VerifyResetCodeDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId, CancellationToken ct = default);
    }
}