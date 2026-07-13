using BuildingBlocks.Results;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Infrastructure.Repositories
{
    public class CloudinaryStorageService : IImageStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryStorageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ServiceResult<string>> UploadImageAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "onpoint_store/products",
                Transformation = new Transformation().Quality("auto:good").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

            if (uploadResult.Error != null)
            {
                return new ServiceResult<string>
                {
                    Succeeded = false,
                    Message = uploadResult.Error.Message,
                    HttpStatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }

            return new ServiceResult<string>
            {
                Succeeded = true,
                Data = uploadResult.SecureUrl.ToString(),
                Message = "Image uploaded successfully.",
                HttpStatusCode = System.Net.HttpStatusCode.Created
            };
        }
    }
}
