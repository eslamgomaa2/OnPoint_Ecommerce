using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.MedioServices
{
    public class MediaService : IMediaService
    {
        private readonly IImageStorageService _imageStorageService;
        private readonly ServiceResultHandler _resultHandler;

        public MediaService(IImageStorageService imageStorageService, ServiceResultHandler resultHandler)
        {
            _imageStorageService = imageStorageService;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<string>> UploadProductImageAsync(FileUploadDto file, CancellationToken ct = default)
        {

            if (file.FileContent == null || file.FileContent.Length == 0)
                return _resultHandler.BadRequest<string>("No file provided.");

            if (file.FileContent.Length > 5 * 1024 * 1024)
                return _resultHandler.BadRequest<string>("File size cannot exceed 5 MB.");


            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return _resultHandler.BadRequest<string>("Only image files are allowed.");


            var result = await _imageStorageService.UploadImageAsync(file.FileContent, file.FileName, ct);


            if (!result.Succeeded)
                return _resultHandler.BadRequest<string>(result.Message ?? "Failed to upload image to cloud.");


            return _resultHandler.Created(result.Data!);

        }

        public async Task<ServiceResult<MultipleUploadResultDto>> UploadProductImagesAsync(List<FileUploadDto> files, CancellationToken ct = default)
        {
            var result = new MultipleUploadResultDto();

            if (files == null || files.Count == 0)
                return _resultHandler.BadRequest<MultipleUploadResultDto>("No files provided.");

            foreach (var file in files)
            {

                if (file.FileContent == null || file.FileContent.Length == 0 || file.FileContent.Length > 5 * 1024 * 1024)
                {
                    result.FailedFiles.Add($"{file.FileName} (Invalid or exceeds 5MB)");
                    continue;
                }


                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    result.FailedFiles.Add($"{file.FileName} (Invalid extension)");
                    continue;
                }


                var uploadResult = await _imageStorageService.UploadImageAsync(file.FileContent, file.FileName, ct);

                if (uploadResult.Succeeded)
                    result.UploadedUrls.Add(uploadResult.Data!);
                else
                    result.FailedFiles.Add(file.FileName);
            }


            return _resultHandler.Success(result);
        }
    }
}
