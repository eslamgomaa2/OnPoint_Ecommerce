using BuildingBlocks.Common.Helpers;
using BuildingBlocks.Results;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.AuthServices.Otp
{
    public class OtpService : IOtpService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;

        public OtpService(IUnitOfWork unitOfWork, ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
        }

        private const int OtpLength = 6;
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
        private const int MaxAttempts = 5;

        public async Task<ServiceResult<string>> GenerateAndStoreOtpAsync(int userId)
        {
            var oldOtps = await _unitOfWork.EmailVerificationOtpRepo.GetUnusedOtpsByUserIdAsync(userId);
            foreach (var old in oldOtps)
            {
                old.IsUsed = true;
            }

            var code = GenerateNumericCode.Generate(OtpLength);

            var otp = new EmailVerificationOtp
            {
                UserId = userId,
                OtpCodeHash = HashingHelper.Hash(code),
                ExpiresAt = DateTime.UtcNow.Add(OtpLifetime),
                CreatedAt = DateTime.UtcNow,
                IsUsed = false,
                AttemptCount = 0
            };

            await _unitOfWork.EmailVerificationOtpRepo.AddAsync(otp);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<string>(code);
        }

        public async Task<ServiceResult<bool>> VerifyOtpAsync(int userId, string code)
        {
            var otp = await _unitOfWork.EmailVerificationOtpRepo.GetLastUnusedOtpAsync(userId);

            if (otp is null)
            {
                return _resultHandler.NotFound<bool>("No valid verification code found. Please request a new one.");
            }

            if (otp.ExpiresAt < DateTime.UtcNow)
            {
                return _resultHandler.BadRequest<bool>("Verification code has expired. Please request a new one.");
            }

            if (otp.AttemptCount >= MaxAttempts)
            {
                otp.IsUsed = true;
                await _unitOfWork.SaveChangesAsync();
                return _resultHandler.BadRequest<bool>("Maximum number of attempts exceeded. Please request a new code.");
            }

            if (!string.Equals(otp.OtpCodeHash, HashingHelper.Hash(code.Trim()), StringComparison.Ordinal))
            {
                otp.AttemptCount++;
                await _unitOfWork.SaveChangesAsync();
                return _resultHandler.BadRequest<bool>("Invalid verification code.");
            }

            otp.IsUsed = true;
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<bool>(true);
        }

        public async Task<ServiceResult<bool>> CanResendAsync(int userId)
        {
            var lastOtp = await _unitOfWork.EmailVerificationOtpRepo.GetLastOtpAsync(userId);

            if (lastOtp is null)
                return _resultHandler.Success<bool>(true);

            if (DateTime.UtcNow - lastOtp.CreatedAt < ResendCooldown)
            {
                return _resultHandler.BadRequest<bool>("Please wait before requesting a new code.");
            }

            return _resultHandler.Success<bool>(true);
        }
    }
}