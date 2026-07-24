using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IEmailVerificationOtpRepo : IGenericRepository<EmailVerificationOtp, int>
    {


        Task<List<EmailVerificationOtp>> GetUnusedOtpsByUserIdAsync(int userId, OtpPurpose purpose);
        Task<EmailVerificationOtp?> GetLastUnusedOtpAsync(int userId, OtpPurpose purpose);
        Task<EmailVerificationOtp?> GetLastOtpAsync(int userId, OtpPurpose purpose);




    }
}
