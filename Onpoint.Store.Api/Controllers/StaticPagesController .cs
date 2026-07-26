using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.StaticPages;
using Onpoint.Store.Application.Services.StaticPage;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaticPagesController : ControllerBase
    {
        private readonly IStaticPageService _staticPageService;

        public StaticPagesController(IStaticPageService staticPageService)
        {
            _staticPageService = staticPageService;
        }

        [HttpGet("privacy-policy")]
        public async Task<IActionResult> GetPrivacyPolicy()
        {
            var result = await _staticPageService.GetByTypeAsync(PageType.Privacy);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("terms-of-use")]
        public async Task<IActionResult> GetTermsOfUse()
        {
            var result = await _staticPageService.GetByTypeAsync(PageType.Terms);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("about-us")]
        public async Task<IActionResult> GetAboutUs()
        {
            var result = await _staticPageService.GetByTypeAsync(PageType.AboutUs);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("contact-us")]
        public async Task<IActionResult> GetContactUs()
        {
            var result = await _staticPageService.GetByTypeAsync(PageType.ContactUs);
            return StatusCode((int)result.HttpStatusCode, result);
        }



        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _staticPageService.GetAllAsync();
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _staticPageService.GetByIdAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaticPageDto dto)
        {
            var result = await _staticPageService.CreateAsync(dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStaticPageDto dto)
        {

            var result = await _staticPageService.UpdateAsync(id, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _staticPageService.DeleteAsync(id);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}