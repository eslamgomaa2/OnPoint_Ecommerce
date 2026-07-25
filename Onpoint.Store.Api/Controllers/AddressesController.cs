using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Address;
using Onpoint.Store.Application.Services.AddressServ;
using System.Security.Claims;

[Route("api/addresses")]
[ApiController]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;
    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    private int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyAddresses(CancellationToken ct = default)
    {
        var result = await _addressService.GetUserAddressesAsync(GetUserId(), ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto, CancellationToken ct = default)
    {
        var result = await _addressService.CreateAsync(GetUserId(), dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateAddressDto dto, CancellationToken ct = default)
    {
        var result = await _addressService.UpdateAsync(GetUserId(), id, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var result = await _addressService.DeleteAsync(GetUserId(), id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}