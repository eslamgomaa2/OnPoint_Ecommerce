using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.Services.MedioServices;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken ct = default)
        {

            using var stream = file.OpenReadStream();

            var dto = new FileUploadDto
            {
                FileContent = stream,
                FileName = file.FileName
            };


            var result = await _mediaService.UploadProductImageAsync(dto, ct);

            return StatusCode((int)result.HttpStatusCode, result);
        }
        [HttpPost("upload-images")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files, CancellationToken ct = default)
        {
            var dtoList = new List<FileUploadDto>();


            foreach (var file in files)
            {
                dtoList.Add(new FileUploadDto
                {
                    FileContent = file.OpenReadStream(),
                    FileName = file.FileName
                });
            }


            var result = await _mediaService.UploadProductImagesAsync(dtoList, ct);

            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}
