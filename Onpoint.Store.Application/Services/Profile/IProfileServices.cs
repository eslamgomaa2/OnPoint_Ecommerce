using BuildingBlocks.Results;

namespace Onpoint.Store.Application.Services.Profile
{
    public interface IProfileServices
    {
        Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId);
    }
}
