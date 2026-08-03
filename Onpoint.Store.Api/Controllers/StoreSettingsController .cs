using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.StoreSettings;
using Onpoint.Store.Application.Services.StoreSettings;

namespace Onpoint.Store.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreSettingsController : ControllerBase
    {
        private readonly IStoreSettingsService _service;

        public StoreSettingsController(IStoreSettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct = default)
        {
            var result = await _service.GetAsync(ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StoreSettingsUpdateDto dto, CancellationToken ct = default)
        {
            var result = await _service.CreateAsync(dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StoreSettingItemDto dto, CancellationToken ct = default)
        {
            var result = await _service.UpdateAsync(id, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var result = await _service.DeleteAsync(id, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}