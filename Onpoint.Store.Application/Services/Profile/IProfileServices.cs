using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Services.Profile
{
    public interface IProfileServices
    {
        Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId);

        Task<ServiceResult<string>> UpdateMyAccount(int UserId, UpdateMyProfileDto dto, CancellationToken ct = default);
    }
}
