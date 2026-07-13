using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.Invoice;
using Onpoint.Store.Application.Services.InvoiceServ;

namespace Onpoint.Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResult<InvoiceDto>>> Generate([FromBody] CreateInvoiceDto dto)
        {
            var result = await _invoiceService.GenerateInvoiceAsync(dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ServiceResult<InvoiceDto>>> GetByOrder(int orderId)
        {
            var result = await _invoiceService.GetByOrderIdAsync(orderId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpGet("{invoiceNumber}")]
        public async Task<ActionResult<ServiceResult<InvoiceDto>>> GetByNumber(string invoiceNumber)
        {
            var result = await _invoiceService.GetByInvoiceNumberAsync(invoiceNumber);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}