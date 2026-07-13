using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
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

        public async Task<List<EmailVerificationOtp>> GetUnusedOtpsByUserIdAsync(int userId)
        {
            return await _dbcontext.EmailVerificationOtps
                .Where(o => o.UserId == userId
                         && !o.IsUsed)
                .ToListAsync();
        }

        public async Task<EmailVerificationOtp?> GetLastUnusedOtpAsync(int userId)
        {
            return await _context.EmailVerificationOtps
                .Where(o => o.UserId == userId
                         && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailVerificationOtp?> GetLastOtpAsync(int userId)
        {
            return await _context.EmailVerificationOtps
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }

}
