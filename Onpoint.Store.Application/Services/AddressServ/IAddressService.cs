using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Address;

namespace Onpoint.Store.Application.Services.AddressServ
{
    public interface IAddressService
    {
        Task<ServiceResult<IReadOnlyList<AddressDto>>> GetUserAddressesAsync(int userId, CancellationToken ct = default);
        Task<ServiceResult<AddressDto>> CreateAsync(int userId, CreateAddressDto dto, CancellationToken ct = default);
        Task<ServiceResult<AddressDto>> UpdateAsync(int userId, int addressId, UpdateAddressDto dto, CancellationToken ct = default);
        Task<ServiceResult<string>> DeleteAsync(int userId, int addressId, CancellationToken ct = default);
    }
}
