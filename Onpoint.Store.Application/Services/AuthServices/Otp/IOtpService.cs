using BuildingBlocks.Results;

namespace Onpoint.Store.Application.Services.AuthServices.Otp
{
    public interface IOtpService
    {
        Task<ServiceResult<string>> GenerateAndStoreOtpAsync(int userId);

        Task<ServiceResult<bool>> VerifyOtpAsync(int userId, string code);

        Task<ServiceResult<bool>> CanResendAsync(int userId);
    }
}