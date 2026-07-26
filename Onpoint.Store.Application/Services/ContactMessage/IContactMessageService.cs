using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Contactmessage;

namespace Onpoint.Store.Application.Services.ContactMessage
{
    public interface IContactMessageService
    {
        Task<ServiceResult<IEnumerable<ContactMessageReadDto>>> GetAllAsync(CancellationToken ct = default);
        Task<ServiceResult<ContactMessageReadDto?>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ServiceResult<IEnumerable<ContactMessageReadDto>>> GetUnresolvedAsync(CancellationToken ct = default);
        Task<ServiceResult<ContactMessageReadDto>> CreateAsync(ContactMessageCreateDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> UpdateAsync(int id, ContactMessageUpdateDto dto, CancellationToken ct = default);
        Task<ServiceResult<bool>> DeleteAsync(int id, CancellationToken ct = default);
    }
}