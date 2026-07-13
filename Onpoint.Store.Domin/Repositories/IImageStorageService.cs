using BuildingBlocks.Results;

namespace Onpoint.Store.Domin.Repositories
{
    public interface IImageStorageService
    {

        Task<ServiceResult<string>> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    }
}
