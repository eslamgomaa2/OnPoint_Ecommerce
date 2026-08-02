using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.AppSettings;
using Onpoint.Store.Application.Services.AppSettingsServ;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/settings")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin" + "," + "ONPointManager")]
    public class AppSettingsController : ControllerBase
    {
        private readonly IAppSettingsService _appSettingsService;

        public AppSettingsController(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var result = await _appSettingsService.GetAsync(ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateAppSettingsDto dto, CancellationToken ct)
        {
            var result = await _appSettingsService.CreateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }


        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] UpdateAppSettingsDto dto, CancellationToken ct)
        {
            var result = await _appSettingsService.UpdateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}