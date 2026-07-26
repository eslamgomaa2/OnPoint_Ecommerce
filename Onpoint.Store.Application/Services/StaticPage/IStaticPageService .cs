using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.StaticPage;
using Onpoint.Store.Application.DTOs.StaticPages;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Services.StaticPage
{
    public interface IStaticPageService
    {
        Task<ServiceResult<StaticPageReadDto>> GetByTypeAsync(PageType type, CancellationToken ct = default);
        Task<ServiceResult<IEnumerable<StaticPageReadDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<StaticPageReadDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<StaticPageReadDto>> CreateAsync(CreateStaticPageDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> UpdateAsync(int id, UpdateStaticPageDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
    }
}