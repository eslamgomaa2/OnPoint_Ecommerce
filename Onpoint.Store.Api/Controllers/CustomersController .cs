using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Customer;
using Onpoint.Store.Application.Services.Customer;
using System.Security.Claims;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "SuperAdmin")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }


    [HttpGet]
    public async Task<ActionResult<ServiceResult<PagedResult<CustomerListItemDto>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.GetAllAsync(branchId.Value, search, isActive, pageNumber, pageSize, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpGet("stats")]
    public async Task<ActionResult<ServiceResult<CustomerStatsDto>>> GetStats(CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.GetStatsAsync(branchId.Value, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpGet("phone/{phone}")]
    public async Task<ActionResult<ServiceResult<CustomerDetailsDto>>> GetByPhoneNumber(string phone, CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.GetByPhoneNumberAsync(phone, branchId.Value, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceResult<CustomerDetailsDto>>> GetById(int id, CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.GetByIdAsync(id, branchId.Value, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpPost]
    public async Task<ActionResult<ServiceResult<CustomerDetailsDto>>> Create(
        [FromBody] CreateCustomerDto dto, CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.CreateAsync(dto, branchId.Value, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServiceResult<CustomerDetailsDto>>> Update(
        int id, [FromBody] UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.UpdateAsync(id, dto, branchId.Value, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }


    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ServiceResult<bool>>> Delete(int id, CancellationToken ct = default)
    {
        var (_, branchId) = GetUserAndBranchId();
        if (branchId is null)
            return BadRequest("Branch not found for current user.");

        var result = await _customerService.DeleteAsync(id, branchId.Value, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    private (int UserId, int? BranchId) GetUserAndBranchId()
    {
        var userClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        var branchClaim = User.FindFirstValue("BranchId");
        if (string.IsNullOrEmpty(userClaim) || !int.TryParse(userClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
        int? branchId = int.TryParse(branchClaim, out var bId) ? bId : null;
        return (userId, branchId);
    }
}