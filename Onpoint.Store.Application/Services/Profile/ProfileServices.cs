using BuildingBlocks.Results;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.Profile
{
    public class ProfileServices : IProfileServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;

        public ProfileServices(IUnitOfWork unitOfWork, ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<bool>> DeleteMyAccountAsync(int userId)
        {
            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId);
            if (user is null) return _resultHandler.NotFound<bool>("User not found");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            _unitOfWork.ApplicationUsers.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Deleted<bool>();
        }
    }
}
