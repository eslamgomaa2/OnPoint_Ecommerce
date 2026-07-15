using BuildingBlocks.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Payment;
using Onpoint.Store.Application.Services.OrderServ;
using Onpoint.Store.Application.Services.PaymentServ;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.PaymentServices
{


    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService orderService;


        private readonly ServiceResultHandler _resultHandler;
        private readonly IMyFatoorahClient _myFatoorahClient;
        private readonly MyFatoorahOptions _options;

        public PaymentService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMyFatoorahClient myFatoorahClient,
            IOptions<MyFatoorahOptions> options,
            UserManager<ApplicationUser> userManager,
            IOrderService orderService)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _myFatoorahClient = myFatoorahClient;
            _options = options.Value;
            _userManager = userManager;
            this.orderService = orderService;
        }

        public async Task<ServiceResult<List<PaymentMethodDto>>> GetAvailablePaymentMethodsAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
            if (order == null)
                return _resultHandler.NotFound<List<PaymentMethodDto>>("Order not found.");

            var methods = await _myFatoorahClient.InitiatePaymentAsync(order.TotalAmount, "KWD", ct);
            return _resultHandler.Success(methods);
        }

        public async Task<ServiceResult<ExecutePaymentResultDto>> PayViaHostedAsync(int userId, PayViaHostedDto dto, CancellationToken ct = default)
        {
            var order = await ValidateOrderForPaymentAsync(userId, dto.OrderId, ct);
            if (order == null)
                return _resultHandler.BadRequest<ExecutePaymentResultDto>("Order is not eligible for payment.");

            var user = await _userManager.FindByIdAsync(userId.ToString());

            var request = new ExecutePaymentRequestModel
            {
                PaymentMethodId = dto.PaymentMethodId,
                InvoiceValue = order.TotalAmount,
                DisplayCurrencyIso = "KWD",
                CustomerName = user?.UserName ?? "Customer",
                CustomerEmail = user?.Email ?? string.Empty,
                CustomerMobile = user?.PhoneNumber ?? string.Empty,
                CallBackUrl = $"{_options.CallBackBaseUrl}?orderId={order.Id}",
                ErrorUrl = $"{_options.ErrorBaseUrl}?orderId={order.Id}",
                CustomerReference = order.Id.ToString()
            };

            var result = await _myFatoorahClient.ExecutePaymentAsync(request, ct);

            await SaveOrUpdatePendingTransactionAsync(order.Id, "MyFatoorah", result.InvoiceId, order.TotalAmount, ct);

            return _resultHandler.Success(result);
        }

        public async Task<ServiceResult<ExecutePaymentResultDto>> PayViaEmbeddedAsync(int userId, PayViaEmbeddedDto dto, CancellationToken ct = default)
        {
            var order = await ValidateOrderForPaymentAsync(userId, dto.OrderId, ct);
            if (order == null)
                return _resultHandler.BadRequest<ExecutePaymentResultDto>("Order is not eligible for payment.");

            var user = await _userManager.FindByIdAsync(userId.ToString());

            var request = new ExecutePaymentRequestModel
            {
                SessionId = dto.SessionId,
                InvoiceValue = order.TotalAmount,
                DisplayCurrencyIso = "KWD",
                CustomerName = user?.UserName ?? "Customer",
                CustomerEmail = user?.Email ?? string.Empty,
                CustomerMobile = user?.PhoneNumber ?? string.Empty,
                CallBackUrl = $"{_options.CallBackBaseUrl}?orderId={order.Id}",
                ErrorUrl = $"{_options.ErrorBaseUrl}?orderId={order.Id}",
                CustomerReference = order.Id.ToString()
            };

            var result = await _myFatoorahClient.ExecutePaymentAsync(request, ct);

            await SaveOrUpdatePendingTransactionAsync(order.Id, "MyFatoorah", result.InvoiceId, order.TotalAmount, ct);

            return _resultHandler.Success(result);
        }


        public async Task HandleWebhookNotificationAsync(string invoiceIdOrPaymentId, CancellationToken ct = default)
        {
            await ProcessPaymentConfirmationAsync(invoiceIdOrPaymentId, ct);
        }

        // ============ Status Check اليدوي (نفس اللوجيك، لكن بيرجع نتيجة واضحة للفرونت إند) ============
        public async Task<ServiceResult<PaymentStatusDto>> CheckPaymentStatusAsync(string invoiceId, CancellationToken ct = default)
        {
            var (order, finalStatus, message) = await ProcessPaymentConfirmationAsync(invoiceId, ct);

            if (order == null)
                return _resultHandler.NotFound<PaymentStatusDto>("Order not found for this invoice.");

            return _resultHandler.Success(new PaymentStatusDto
            {
                OrderId = order.Id,
                Status = finalStatus,
                Message = message
            });
        }

        // ============ اللوجيك المشتركة الفعلية (بتتنادى من الاتنين فوق) ============
        private async Task<(Order? Order, string Status, string Message)> ProcessPaymentConfirmationAsync(string invoiceId, CancellationToken ct)
        {
            var status = await _myFatoorahClient.GetPaymentStatusAsync(invoiceId, "InvoiceId", ct);

            if (string.IsNullOrEmpty(status.CustomerReference) || !int.TryParse(status.CustomerReference, out var orderId))
                return (null, "Unknown", "Could not resolve order from payment reference.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
                if (order == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return (null, "Unknown", "Order not found.");
                }

                if (order.Status != OrderStatus.Pending)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    var alreadyStatus = order.Status == OrderStatus.Processing ? "Paid" : "Pending";
                    return (order, alreadyStatus, "Order already processed.");
                }

                var transaction = await _unitOfWork.PaymentTransactions.GetByOrderIdAsync(orderId);

                if (status.InvoiceStatus == "Paid")
                {
                    if (transaction != null)
                    {
                        transaction.GatewayTransactionId = status.InvoiceTransactions?.TransactionId;
                        transaction.Status = "Success";
                        transaction.PaidAt = DateTime.UtcNow;
                        _unitOfWork.PaymentTransactions.Update(transaction);
                    }

                    order.Status = OrderStatus.Processing;
                    _unitOfWork.Orders.Update(order);

                    var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(order.UserId, ct);

                    Coupon? coupon = null;
                    if (order.DiscountAmount > 0 && cart != null && !string.IsNullOrEmpty(cart.AppliedCouponCode))
                        coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode, ct);

                    await orderService.FinalizeOrderAsync(order, cart, coupon, ct);

                    await _unitOfWork.SaveChangesAsync(ct);
                    await _unitOfWork.CommitTransactionAsync();

                    return (order, "Paid", "Payment confirmed and order finalized.");
                }
                else if (status.InvoiceStatus == "Failed" || status.InvoiceStatus == "Expired")
                {
                    if (transaction != null)
                    {
                        transaction.Status = "Failed";
                        transaction.ErrorMessage = status.InvoiceTransactions?.Error;
                        _unitOfWork.PaymentTransactions.Update(transaction);
                        await _unitOfWork.SaveChangesAsync(ct);
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    return (order, "Failed", "Payment failed. You can try again.");
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return (order, "Pending", "Payment is still being processed.");
                }
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        private async Task<Order?> ValidateOrderForPaymentAsync(int userId, int orderId, CancellationToken ct)
        {
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
            if (order == null || order.UserId != userId) return null;
            if (order.Status != OrderStatus.Pending) return null;
            if (order.PaymentMethod == PaymentMethod.CashOnDelivery) return null;
            return order;
        }

        private async Task SaveOrUpdatePendingTransactionAsync(int orderId, string provider, string gatewayId, decimal amount, CancellationToken ct)
        {
            var existing = await _unitOfWork.PaymentTransactions.GetByOrderIdAsync(orderId);

            if (existing != null)
            {
                existing.GatewayTransactionId = gatewayId;
                existing.Status = "Pending";
                _unitOfWork.PaymentTransactions.Update(existing);
            }
            else
            {
                await _unitOfWork.PaymentTransactions.AddAsync(new PaymentTransaction
                {
                    OrderId = orderId,
                    Provider = provider,
                    GatewayTransactionId = gatewayId,
                    Status = "Pending",
                    Amount = amount,
                    CurrencyCode = "KWD"
                }, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}

