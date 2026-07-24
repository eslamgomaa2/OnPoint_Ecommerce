using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{


    public class EmailVerificationOtpRepo : GenericRepository<EmailVerificationOtp, int>, IEmailVerificationOtpRepo
    {
        private readonly ApplicationDbContext _dbcontext;

        public EmailVerificationOtpRepo(ApplicationDbContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<List<EmailVerificationOtp>> GetUnusedOtpsByUserIdAsync(int userId, OtpPurpose purpose)
        {
            return await _context.EmailVerificationOtps
                .Where(o => o.UserId == userId
                         && !o.IsUsed
                         && o.otpPurpose == purpose)
                .ToListAsync();
        }

        public async Task<EmailVerificationOtp?> GetLastUnusedOtpAsync(int userId, OtpPurpose purpose)
        {
            return await _context.EmailVerificationOtps
                .Where(o => o.UserId == userId
                         && !o.IsUsed
                         && o.otpPurpose == purpose)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailVerificationOtp?> GetLastOtpAsync(int userId, OtpPurpose purpose)
        {
            return await _context.EmailVerificationOtps
                .Where(o => o.UserId == userId
                         && o.otpPurpose == purpose)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }

}
