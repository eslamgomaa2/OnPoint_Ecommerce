using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Media;
using Onpoint.Store.Application.Services.MedioServices;

[Route("api/media")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost("images")]
    public async Task<IActionResult> UploadImages(List<IFormFile> files, CancellationToken ct = default)
    {
        var dtoList = new List<FileUploadDto>();

        foreach (var file in files)
        {
            var stream = file.OpenReadStream();
            dtoList.Add(new FileUploadDto
            {
                FileContent = stream,
                FileName = file.FileName
            });
        }

        try
        {
            var result = await _mediaService.UploadProductImagesAsync(dtoList, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
        finally
        {
            foreach (var dto in dtoList)
                dto.FileContent?.Dispose();
        }
    }
}