using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.StoreSettings;

namespace Onpoint.Store.Application.Services.StoreSettings
{
    public interface IStoreSettingsService
    {
        Task<ServiceResult<List<StoreSettingDto>>> GetAsync(CancellationToken ct = default);
        Task<ServiceResult<StoreSettingsUpdateDto>> CreateAsync(StoreSettingsUpdateDto dto, CancellationToken ct = default);
        Task<ServiceResult<StoreSettingDto>> UpdateAsync(int id, StoreSettingItemDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
    }
}