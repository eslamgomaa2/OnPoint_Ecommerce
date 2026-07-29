
using Onpoint.Store.Domin.Entities.CustomerEngagement;

namespace Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
    Task<Notification> CreateAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    Task<(IEnumerable<Notification> Items, int TotalCount)> GetByUserIdAsync(
    int userId,
    int pageNumber,
    int pageSize,
    string? searchTerm,
    bool? isRead,
    CancellationToken cancellationToken = default);


}