using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IEmailVerificationOtpRepo : IGenericRepository<EmailVerificationOtp, int>
    {


        Task<List<EmailVerificationOtp>> GetUnusedOtpsByUserIdAsync(int userId);
        Task<EmailVerificationOtp?> GetLastUnusedOtpAsync(int userId);
        Task<EmailVerificationOtp?> GetLastOtpAsync(int userId);




    }
}
