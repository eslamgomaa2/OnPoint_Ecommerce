using Onpoint.Store.Domin.Entities;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IDiscountRepo : IGenericRepository<Discount, int>
    {

        Task<bool> HasOverlappingDiscountAsync(int productId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
        Task<List<Discount>> GetDiscountsByProductAsync(int productId, CancellationToken ct = default);
        Task<(List<Discount> Items, int TotalCount)> GetAllPagedAsync(
               int? productId,
               string? searchTerm,
               bool? isActive,
               int pageNumber,
               int pageSize,
               CancellationToken ct = default);
    }
}


