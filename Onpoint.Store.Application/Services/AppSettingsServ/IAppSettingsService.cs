using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.AppSettings;

namespace Onpoint.Store.Application.Services.AppSettingsServ
{
    public interface IAppSettingsService
    {
        Task<ServiceResult<AppSettingsDto>> GetAsync(CancellationToken ct = default);
        Task<ServiceResult<AppSettingsDto>> CreateAsync(CreateAppSettingsDto dto, CancellationToken ct = default);
        Task<ServiceResult<AppSettingsDto>> UpdateAsync(UpdateAppSettingsDto dto, CancellationToken ct = default);
    }
}
