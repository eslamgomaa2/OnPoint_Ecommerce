using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Domin.Entities.CustomerEngagement;
using Onpoint.Store.Infrastructure.Data.Context;

namespace Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Notification> _dbSet;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Notification>();
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<(IEnumerable<Notification> Items, int TotalCount)> GetByUserIdAsync(
      int userId,
      int pageNumber,
      int pageSize,
      string? searchTerm,
      bool? isRead,
      CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(n => n.UserId == userId)
            .AsNoTracking();

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(n =>
                n.Title.ToLower().Contains(term) ||
                n.Body.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        await _dbSet.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateAsync(Notification notification)
    {
        _dbSet.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(n => n.Id == id);
    }
}