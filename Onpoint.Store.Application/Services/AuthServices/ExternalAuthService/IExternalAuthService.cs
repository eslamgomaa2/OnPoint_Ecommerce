using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Services.AuthServices.ExternalAuthService
{
    public interface IExternalAuthService
    {
        Task<ServiceResult<AuthResponseDto>> ExternalLoginAsync(ExternalLoginDto dto, CancellationToken ct = default);
    }
}
