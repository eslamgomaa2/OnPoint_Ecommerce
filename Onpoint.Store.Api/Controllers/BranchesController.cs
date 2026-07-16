using BuildingBlocks.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Branch;
using Onpoint.Store.Application.Services.BranchServ;

namespace Onpoint.Store.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchesController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<ServiceResult<IEnumerable<BranchDto>>>> GetAll()
        {
            var result = await _branchService.GetAllAsync();
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ServiceResult<BranchDto>>> GetById(int id)
        {
            var result = await _branchService.GetByIdAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ServiceResult<BranchDto>>> Create([FromBody] CreateBranchDto dto)
        {
            var result = await _branchService.CreateAsync(dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("[action]")]
        public async Task<ActionResult<ServiceResult<BranchDto>>> Update([FromRoute] int Id, [FromBody] UpdateBranchDto dto)
        {
            var result = await _branchService.UpdateAsync(Id, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPatch("[action]/{id}")]
        public async Task<ActionResult<ServiceResult<bool>>> ToggleActive(int id)
        {
            var result = await _branchService.ToggleActiveAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPatch("[action]/{id}")]
        public async Task<ActionResult<ServiceResult<bool>>> SetDefault(int id)
        {
            var result = await _branchService.SetDefaultAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}