using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Address;
using Onpoint.Store.Application.Services.AddressServ;
using System.Security.Claims;

namespace Onpoint.Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

        [HttpGet("GetMyAddresses")]
        public async Task<IActionResult> GetMyAddresses(CancellationToken ct = default)
        {
            var result = await _addressService.GetUserAddressesAsync(GetUserId(), ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateAddressDto dto, CancellationToken ct = default)
        {
            var result = await _addressService.CreateAsync(GetUserId(), dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAddressDto dto, CancellationToken ct = default)
        {
            var result = await _addressService.UpdateAsync(GetUserId(), id, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("Delete/{addressId}")]
        public async Task<IActionResult> Delete(int addressId, CancellationToken ct = default)
        {
            var result = await _addressService.DeleteAsync(GetUserId(), addressId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

    }
}