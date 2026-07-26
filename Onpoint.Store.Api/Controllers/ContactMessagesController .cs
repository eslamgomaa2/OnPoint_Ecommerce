using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Contactmessage;
using Onpoint.Store.Application.Services.ContactMessage;

namespace ContactMessagesCrud.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactMessagesController : ControllerBase
    {
        private readonly IContactMessageService _contactMessageService;

        public ContactMessagesController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        [HttpGet]
        public async Task<ActionResult<ServiceResult<IEnumerable<ContactMessageReadDto>>>> GetAll()
        {
            var result = await _contactMessageService.GetAllAsync();
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("unresolved")]
        public async Task<ActionResult<ServiceResult<IEnumerable<ContactMessageReadDto>>>> GetUnresolved()
        {
            var result = await _contactMessageService.GetUnresolvedAsync();
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceResult<ContactMessageReadDto?>>> GetById(int id)
        {
            var result = await _contactMessageService.GetByIdAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResult<ContactMessageReadDto>>> Create([FromBody] ContactMessageCreateDto dto)
        {
            var result = await _contactMessageService.CreateAsync(dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResult<bool>>> Update(int id, [FromBody] ContactMessageUpdateDto dto)
        {


            var result = await _contactMessageService.UpdateAsync(id, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ServiceResult<bool>>> Delete(int id)
        {
            var result = await _contactMessageService.DeleteAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}