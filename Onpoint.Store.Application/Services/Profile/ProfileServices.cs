using BuildingBlocks.Results;
using Microsoft.AspNetCore.Identity;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.DTOs.Auth.Profile;
using Onpoint.Store.Application.Services.AuthServices.Token;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.Profile
{
    public class ProfileServices : IProfileServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _resultHandler;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public ProfileServices(IUnitOfWork unitOfWork, ServiceResultHandler resultHandler, UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _userManager = userManager;
            _tokenService = tokenService;
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

        public async Task<ServiceResult<UpdateMyProfileDto>> UpdateMyAccount(int UserId, UpdateMyProfileDto dto, CancellationToken ct = default)
        {
            var user = await _userManager.FindByIdAsync(UserId.ToString());
            if (user == null)
                return _resultHandler.NotFound<UpdateMyProfileDto>("User not found.");

            if (!string.IsNullOrEmpty(dto.FullName))
            {
                user.UserName = dto.FullName;
            }

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return _resultHandler.BadRequest<UpdateMyProfileDto>(errors);
            }

            return _resultHandler.Success(dto);
        }

        public async Task<ServiceResult<UserLoginInfoDto>> GetMyLoginInfoAsync(int userId, CancellationToken ct = default)
        {

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return _resultHandler.NotFound<UserLoginInfoDto>("User not found.");






            string? branchName = null;
            if (user.BranchId.HasValue)
            {
                var branch = await _unitOfWork.Branches.GetByIdAsync(user.BranchId.Value, ct);
                branchName = branch?.Name;
            }




            var dto = new UserLoginInfoDto
            {
                Id = user.Id,
                FullName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,

                BranchName = branchName,


            };

            return _resultHandler.Success(dto);
        }
    }
}