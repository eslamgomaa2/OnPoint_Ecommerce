using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.DTOs.Auth.Profile;

namespace Onpoint.Store.Application.Services.Profile
{
    public interface IProfileServices
    {
        Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId);

        Task<ServiceResult<UpdateMyProfileDto>> UpdateMyAccount(int UserId, UpdateMyProfileDto dto, CancellationToken ct = default);
        Task<ServiceResult<UserLoginInfoDto>> GetMyLoginInfoAsync(int userId, CancellationToken ct = default);
    }
}
