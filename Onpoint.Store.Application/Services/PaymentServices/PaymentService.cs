using BuildingBlocks.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Payment;
using Onpoint.Store.Application.Services.OrderServ;
using Onpoint.Store.Application.Services.PaymentServ;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using System.Data;

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
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            ServiceResultHandler resultHandler,
            IMyFatoorahClient myFatoorahClient,
            IOptions<MyFatoorahOptions> options,
            UserManager<ApplicationUser> userManager,
            IOrderService orderService,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _resultHandler = resultHandler;
            _myFatoorahClient = myFatoorahClient;
            _options = options.Value;
            _userManager = userManager;
            this.orderService = orderService;
            _logger = logger;
        }

        public async Task<ServiceResult<List<PaymentMethodDto>>> GetAvailablePaymentMethodsAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, ct);
            if (order == null)
                return _resultHandler.NotFound<List<PaymentMethodDto>>("Order not found.");

            var methods = await _myFatoorahClient.InitiatePaymentAsync(order.TotalAmount, "KWD", ct);
            return _resultHandler.Success<List<PaymentMethodDto>>(methods);
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

            return _resultHandler.Success<ExecutePaymentResultDto>(result);
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

            return _resultHandler.Success<ExecutePaymentResultDto>(result);
        }

        public async Task HandleWebhookNotificationAsync(string invoiceId, CancellationToken ct = default)
        {
            var statusResult = await _myFatoorahClient
                .GetPaymentStatusAsync(invoiceId, "InvoiceId", ct);

            if (statusResult.InvoiceStatus != "Paid")
            {
                _logger.LogWarning("MyFatoorah invoice {InvoiceId} status: {Status}",
                    invoiceId, statusResult.InvoiceStatus);

                if (statusResult.InvoiceStatus == "Failed" ||
                    statusResult.InvoiceStatus == "Expired")
                {
                    await HandlePaymentFailureAsync(invoiceId, statusResult, ct: ct);
                }
                return;
            }

            if (!int.TryParse(statusResult.CustomerReference, out var orderId))
            {
                _logger.LogError("Invalid CustomerReference in MyFatoorah response");
                return;
            }

            var order = await _unitOfWork.Orders
                .GetOrderWithItemsAsync(orderId, ct);

            if (order == null || order.Status != OrderStatus.PendingPayment)
                return;

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                // ✅ اخصم Stock
                foreach (var item in order.OrderItems)
                {
                    var stock = await _unitOfWork.Stocks
                        .GetByProductVariantAndBranchAsync(
                            item.ProductId,
                            item.ProductVariantId,
                            order.BranchId);

                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        await HandlePaymentFailureAsync(invoiceId, statusResult, "Insufficient stock", ct);
                        return;
                    }

                    stock.Quantity -= item.Quantity;
                    _unitOfWork.Stocks.Update(stock);
                }

                var allTransactions = await _unitOfWork.PaymentTransactions
                    .GetAllAsync(ct); // أو اللي عندك

                var cardTransaction = allTransactions
                    .FirstOrDefault(t => t.GatewayTransactionId == invoiceId
                                      && t.Status == PaymentStatus.Pending);

                // ✅ لو مش لاقي بالـ GatewayTransactionId، دور بالـ OrderId + Pending
                if (cardTransaction == null)
                {
                    cardTransaction = allTransactions
                        .FirstOrDefault(t => t.OrderId == order.Id
                                          && t.Status == PaymentStatus.Pending
                                          && t.PaymentMethod != PaymentMethod.Cash);
                }

                if (cardTransaction != null)
                {
                    cardTransaction.Status = PaymentStatus.Success;
                    cardTransaction.PaidAt = DateTime.UtcNow;
                    cardTransaction.GatewayTransactionId = statusResult.InvoiceTransactions?.TransactionId
                                                        ?? cardTransaction.GatewayTransactionId;
                    _unitOfWork.PaymentTransactions.Update(cardTransaction);
                }
                else
                {
                    _logger.LogWarning("No pending card transaction found for Order {OrderId}", order.Id);
                }

                order.Status = OrderStatus.Completed;
                _unitOfWork.Orders.Update(order);

                // ✅ جرّب GetByOrderIdAsync الأول (لو موجودة)
                PosSession? session = null;
                try
                {
                    var sessions = await _unitOfWork.PosSessions.GetAllAsync(ct);
                    session = sessions.FirstOrDefault(s => s.OrderId == order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not find session by OrderId");
                }

                if (session != null)
                {
                    session.Status = PosSessionStatus.Completed;
                    _unitOfWork.PosSessions.Update(session);
                }

                order.StatusHistory.Add(new OrderStatusHistory
                {
                    Status = OrderStatus.Completed,
                    EventName = EventName.PaymentReceived,
                    Description = $"Card payment received via MyFatoorah. Invoice: {invoiceId}",
                    EventTime = DateTime.UtcNow,
                    PerformedByUserId = order.CashierId
                });

                order.StatusHistory.Add(new OrderStatusHistory
                {
                    Status = OrderStatus.Completed,
                    EventName = EventName.ReceiptPrinted,
                    Description = "Receipt printed",
                    EventTime = DateTime.UtcNow,
                    PerformedByUserId = order.CashierId
                });

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Order {OrderId} completed successfully via MyFatoorah", orderId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Failed to process webhook for Order {OrderId}", orderId);
                throw;
            }
        }
        public async Task<ServiceResult<PaymentStatusDto>> CheckPaymentStatusAsync(string invoiceId, CancellationToken ct = default)
        {
            var (order, finalStatus, message) = await ProcessPaymentConfirmationAsync(invoiceId, ct);

            if (order == null)
                return _resultHandler.NotFound<PaymentStatusDto>("Order not found for this invoice.");

            var dto = new PaymentStatusDto
            {
                OrderId = order.Id,
                Status = finalStatus,
                Message = message
            };

            return _resultHandler.Success<PaymentStatusDto>(dto);
        }

        private async Task<(Order? Order, string Status, string Message)> ProcessPaymentConfirmationAsync(string invoiceId, CancellationToken ct)
        {
            var status = await _myFatoorahClient.GetPaymentStatusAsync(invoiceId, "InvoiceId", ct);

            if (string.IsNullOrEmpty(status.CustomerReference) || !int.TryParse(status.CustomerReference, out var orderId))
                return (null, "Unknown", "Could not resolve order from payment reference.");

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
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
                    var alreadyStatus = order.Status.ToString();
                    return (order, alreadyStatus, "Order already processed.");
                }

                var transaction = await _unitOfWork.PaymentTransactions.GetByOrderIdAsync(orderId);

                if (status.InvoiceStatus == "Paid")
                {
                    if (transaction != null)
                    {
                        transaction.GatewayTransactionId = status.InvoiceTransactions?.TransactionId;
                        transaction.Status = PaymentStatus.Success;
                        transaction.PaidAt = DateTime.UtcNow;
                        _unitOfWork.PaymentTransactions.Update(transaction);
                    }

                    order.Status = OrderStatus.Pending;
                    _unitOfWork.Orders.Update(order);

                    var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(order.UserId!.Value, ct);

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
                        transaction.Status = PaymentStatus.Failed;
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

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found.", orderId);
                return null;
            }
            if (order.UserId != userId)
            {
                _logger.LogWarning("Order {OrderId} UserId {OrderUserId} does not match requesting userId {UserId}.", orderId, order.UserId, userId);
                return null;
            }
            if (order.Status != OrderStatus.Pending)
            {
                _logger.LogWarning("Order {OrderId} status is {Status}, expected Pending.", orderId, order.Status);
                return null;
            }
            if (order.PaymentMethod == PaymentMethod.Cash)
            {
                _logger.LogWarning("Order {OrderId} payment method is Cash, not eligible for online payment.", orderId);
                return null;
            }

            return order;
        }

        private async Task SaveOrUpdatePendingTransactionAsync(int orderId, string provider, string gatewayId, decimal amount, CancellationToken ct)
        {
            var existing = await _unitOfWork.PaymentTransactions.GetByOrderIdAsync(orderId);

            if (existing != null)
            {
                existing.GatewayTransactionId = gatewayId;
                existing.Status = PaymentStatus.Pending;
                _unitOfWork.PaymentTransactions.Update(existing);
            }
            else
            {
                await _unitOfWork.PaymentTransactions.AddAsync(new PaymentTransaction
                {
                    OrderId = orderId,
                    Provider = provider,
                    GatewayTransactionId = gatewayId,
                    Status = PaymentStatus.Pending,
                    Amount = amount,
                    CurrencyCode = "KWD"
                }, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
        private async Task HandlePaymentFailureAsync(
     string invoiceId,
     MyFatoorahPaymentStatusResult statusResult,
     string? reason = null,
     CancellationToken ct = default)
        {
            try
            {
                if (!int.TryParse(statusResult.CustomerReference, out var orderId))
                {
                    _logger.LogError("Invalid CustomerReference for failed payment");
                    return;
                }

                var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
                if (order == null) return;

                await _unitOfWork.BeginTransactionAsync(isolationLevel: IsolationLevel.Serializable);
                try
                {
                    order.Status = OrderStatus.PaymentFailed;
                    _unitOfWork.Orders.Update(order);

                    // ✅ ابحث بالـ GatewayTransactionId
                    var allTransactions = await _unitOfWork.PaymentTransactions.GetAllAsync(ct);
                    var transaction = allTransactions
                        .FirstOrDefault(t => t.GatewayTransactionId == invoiceId);

                    if (transaction != null)
                    {
                        transaction.Status = PaymentStatus.Failed;
                        transaction.ErrorMessage = reason ?? statusResult.InvoiceTransactions?.Error;
                        _unitOfWork.PaymentTransactions.Update(transaction);
                    }

                    var sessions = await _unitOfWork.PosSessions.GetAllAsync(ct);
                    var session = sessions.FirstOrDefault(s => s.OrderId == orderId);

                    if (session != null)
                    {
                        session.Status = PosSessionStatus.PaymentFailed;
                        _unitOfWork.PosSessions.Update(session);
                    }

                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        Status = OrderStatus.PaymentFailed,
                        EventName = EventName.PaymentFailed,
                        Description = $"Card payment failed. Reason: {reason ?? statusResult.InvoiceTransactions?.Error ?? "Unknown"}",
                        EventTime = DateTime.UtcNow,
                        PerformedByUserId = order.CashierId
                    });

                    await _unitOfWork.SaveChangesAsync(ct);
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogWarning("Order {OrderId} marked as PaymentFailed", order.Id);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle payment failure for invoice {InvoiceId}", invoiceId);
            }
        }
    }
}