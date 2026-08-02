using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Coupon;
using Onpoint.Store.Application.Services.CouponServ;

[Route("api/coupons")]
[ApiController]
[Authorize(Roles = "SuperAdmin" + "," + "BranchManager")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;
    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CouponFilterRequest filter, CancellationToken ct = default)
    {
        var result = await _couponService.GetAllAsync(filter, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResult<CouponDto>>> GetById(int id)
    {
        var result = await _couponService.GetByIdAsync(id);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<CouponDto>>> Create([FromBody] CreateCouponDto dto)
    {
        var result = await _couponService.CreateAsync(dto);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServiceResult<CouponDto>>> Update([FromRoute] int id, [FromBody] UpdateCouponDto dto)
    {
        var result = await _couponService.UpdateAsync(id, dto);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ServiceResult<bool>>> Delete(int id)
    {
        var result = await _couponService.DeleteAsync(id);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}