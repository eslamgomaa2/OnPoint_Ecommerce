using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.PaymentTransaction;
using Onpoint.Store.Application.Services.PaymentTransactionServ;

namespace Onpoint.Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTransactionsController : ControllerBase
    {
        private readonly IPaymentTransactionService _paymentService;

        public PaymentTransactionsController(IPaymentTransactionService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<ServiceResult<PaymentTransactionDto>>> GetByOrder(int orderId)
        {
            var result = await _paymentService.GetByOrderIdAsync(orderId);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ServiceResult<PaymentTransactionDto>>> UpdateStatus(int id, [FromBody] UpdatePaymentStatusDto dto)
        {
            var result = await _paymentService.UpdateStatusAsync(id, dto);
            return StatusCode((int)result.HttpStatusCode, result);
        }
    }
}