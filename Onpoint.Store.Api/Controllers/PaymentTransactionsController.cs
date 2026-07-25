using BuildingBlocks.Results;
using Microsoft.AspNetCore.Mvc;
using Onpoint.Store.Application.DTOs.PaymentTransaction;
using Onpoint.Store.Application.Services.PaymentTransactionServ;

[Route("api/payment-transactions")]
[ApiController]
public class PaymentTransactionsController : ControllerBase
{
    private readonly IPaymentTransactionService _paymentService;
    public PaymentTransactionsController(IPaymentTransactionService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("/api/orders/{orderId}/payment-transactions")]
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