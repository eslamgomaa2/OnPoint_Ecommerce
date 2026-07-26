using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class ContactMessageRepository : GenericRepository<ContactMessage, int>, IContactMessageRepository
    {
        public ContactMessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ContactMessage>> GetUnresolvedAsync() =>
            await _dbset.Where(m => !m.IsResolved)
                        .OrderByDescending(m => m.CreatedAt)
                        .ToListAsync();

        public async Task<IEnumerable<ContactMessage>> GetByEmailAsync(string email) =>
            await _dbset.Where(m => m.Email.ToLower() == email.ToLower())
                        .OrderByDescending(m => m.CreatedAt)
                        .ToListAsync();
    }
}
