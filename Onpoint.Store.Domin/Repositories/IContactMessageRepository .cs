using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IContactMessageRepository : IGenericRepository<ContactMessage, int>
    {
        Task<IEnumerable<ContactMessage>> GetUnresolvedAsync();
        Task<IEnumerable<ContactMessage>> GetByEmailAsync(string email);
    }
}
