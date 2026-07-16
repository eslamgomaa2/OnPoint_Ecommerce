using BuildingBlocks.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Payment;
using Onpoint.Store.Application.Services.PaymentServices;
using System.Security.Claims;
using System.Text.Json;

namespace Onpoint.Store.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly string _webhookSecret;

        public PaymentController(IPaymentService paymentService, IOptions<MyFatoorahOptions> options)
        {
            _paymentService = paymentService;
            _webhookSecret = options.Value.WebhookSecret;
        }

        private int GetUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user identifier in token.");
            return userId;
        }

        [Authorize]
        [HttpGet("{orderId}/methods")]
        public async Task<IActionResult> GetAvailableMethods(int orderId, CancellationToken ct = default)
        {
            var result = await _paymentService.GetAvailablePaymentMethodsAsync(orderId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [Authorize]
        [HttpPost("pay-hosted")]
        public async Task<IActionResult> PayViaHosted([FromBody] PayViaHostedDto dto, CancellationToken ct = default)
        {
            var userId = GetUserId();
            var result = await _paymentService.PayViaHostedAsync(userId, dto, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }

        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> CheckPaymentStatus([FromQuery] string invoiceId, CancellationToken ct = default)
        {
            var result = await _paymentService.CheckPaymentStatusAsync(invoiceId, ct);
            return StatusCode((int)result.HttpStatusCode, result);
        }



        // ============ Webhook (بيتنادى من MyFatoorah مباشرة، مش من الفرونت إند) ============
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(CancellationToken ct = default)
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;

            var signatureHeader = Request.Headers["MyFatoorah-Signature"].ToString();

            if (!MyFatoorahSignatureVerifier.Verify(rawBody, signatureHeader, _webhookSecret))
                return Unauthorized();

            var invoiceId = ExtractInvoiceIdFromPayload(rawBody);
            await _paymentService.HandleWebhookNotificationAsync(invoiceId.ToString(), ct);
            return Ok();
        }

        private int ExtractInvoiceIdFromPayload(string rawBody)
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("Event", out var eventElement) &&
                eventElement.TryGetProperty("InvoiceId", out var invoiceIdElement))
            {
                return invoiceIdElement.GetInt32();
            }
            throw new InvalidOperationException("Cannot extract InvoiceId from MyFatoorah webhook payload.");
        }




    }
}