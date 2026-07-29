using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Discount;
using Onpoint.Store.Application.Services.DiscountServ;

[Route("api/discounts")]
[ApiController]
[Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountService _discountService;
    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }


    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddDiscountDto dto, CancellationToken ct = default)
    {
        var result = await _discountService.AddDiscountAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }





    [HttpPatch("{discountId}/deactivate")]
    public async Task<IActionResult> Deactivate(int discountId, CancellationToken ct = default)
    {
        var result = await _discountService.DeactivateAsync(discountId, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}