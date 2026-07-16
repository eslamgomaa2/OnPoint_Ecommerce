
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Branch;

namespace Onpoint.Store.Application.Services.BranchServ
{
    public interface IBranchService
    {
        Task<ServiceResult<IEnumerable<BranchDto>>> GetAllAsync();
        Task<ServiceResult<BranchDto>> GetByIdAsync(int id);
        Task<ServiceResult<BranchDto>> CreateAsync(CreateBranchDto dto);
        Task<ServiceResult<BranchDto>> UpdateAsync(int Id, UpdateBranchDto dto);
        Task<ServiceResult<bool>> ToggleActiveAsync(int id);
        Task<ServiceResult<bool>> SetDefaultAsync(int id);
    }
}