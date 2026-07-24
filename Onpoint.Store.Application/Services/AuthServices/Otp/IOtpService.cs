using BuildingBlocks.Results;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.AuthServices.Otp
{
    public interface IOtpService
    {
        Task<ServiceResult<string>> GenerateAndStoreOtpAsync(int userId, OtpPurpose purpose);

        Task<ServiceResult<bool>> VerifyOtpAsync(int userId, string code, OtpPurpose purpose);

        Task<ServiceResult<bool>> CanResendAsync(int userId, OtpPurpose purpose);
    }
}