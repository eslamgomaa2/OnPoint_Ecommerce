using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Media;

namespace Onpoint.Store.Application.Services.MedioServices
{
    public interface IMediaService
    {
        Task<ServiceResult<string>> UploadProductImageAsync(FileUploadDto file, CancellationToken ct = default);
        Task<ServiceResult<MultipleUploadResultDto>> UploadProductImagesAsync(List<FileUploadDto> files, CancellationToken ct = default);

    }
}
