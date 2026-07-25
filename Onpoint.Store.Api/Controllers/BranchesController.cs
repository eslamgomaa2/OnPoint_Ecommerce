using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Auth;
using Onpoint.Store.Application.DTOs.Branch;
using Onpoint.Store.Application.Services.BranchServ;

[ApiController]
[Route("api/branches")]
[Authorize(Roles = "SuperAdmin")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;
    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] BranchPagedRequestDto request, CancellationToken ct)
    {
        var result = await _branchService.GetPagedAsync(request, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _branchService.GetByIdAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto, CancellationToken ct)
    {
        var result = await _branchService.CreateAsync(dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto, CancellationToken ct)
    {
        var result = await _branchService.UpdateAsync(id, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _branchService.DeleteAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken ct)
    {
        var result = await _branchService.ToggleActiveAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPatch("{id:int}/set-default")]
    public async Task<IActionResult> SetDefault(int id, CancellationToken ct)
    {
        var result = await _branchService.SetDefaultAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpPost("{id:int}/manager")]
    public async Task<IActionResult> AssignManager(int id, [FromBody] CreateManagerDto dto, CancellationToken ct)
    {
        var result = await _branchService.AssignManagerAsync(id, dto, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }

    [HttpDelete("{id:int}/manager")]
    public async Task<IActionResult> RemoveManager(int id, CancellationToken ct)
    {
        var result = await _branchService.RemoveManagerAsync(id, ct);
        return StatusCode((int)result.HttpStatusCode, result);
    }
}