using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Branch;

namespace Onpoint.Store.Application.Services.BranchServ
{
    public interface IBranchService
    {
        Task<ServiceResult<IEnumerable<BranchDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<BranchDto>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<BranchDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default);
        Task<ServiceResult<BranchDto>> UpdateAsync(int id, UpdateBranchDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> ToggleActiveAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<bool>> SetDefaultAsync(int id, CancellationToken ct = default);
    }
}