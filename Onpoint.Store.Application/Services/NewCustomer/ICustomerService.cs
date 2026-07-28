using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Customer;

namespace Onpoint.Store.Application.Services.Customer
{
    public interface ICustomerService
    {
        Task<ServiceResult<PagedResult<CustomerListItemDto>>> GetAllAsync(
            int? branchId,
            string? search,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<ServiceResult<CustomerDetailsDto>> GetByIdAsync(int id, int? branchId, CancellationToken ct = default);
        Task<ServiceResult<CustomerDetailsDto>> GetByPhoneNumberAsync(string phone, int? branchId, CancellationToken ct = default);

        Task<ServiceResult<CustomerStatsDto>> GetStatsAsync(int? branchId, CancellationToken ct = default);

        Task<ServiceResult<CustomerDetailsDto>> CreateAsync(CreateCustomerDto dto, int? branchId, CancellationToken ct = default);

        Task<ServiceResult<CustomerDetailsDto>> UpdateAsync(int id, UpdateCustomerDto dto, int? branchId, CancellationToken ct = default);

        Task<ServiceResult<bool>> DeleteAsync(int id, int? branchId, CancellationToken ct = default);
    }
}