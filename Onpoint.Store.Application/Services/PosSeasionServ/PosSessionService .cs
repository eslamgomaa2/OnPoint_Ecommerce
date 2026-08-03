using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Onpoint.Store.Application.DTOs.Customer;
using Onpoint.Store.Application.DTOs.Pos;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Application.Services.CodeGeneration.QrCodeGeneration;
using Onpoint.Store.Application.Services.PaymentServ;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using System.Data;

namespace Onpoint.Store.Application.Services.PosServ
{
    public class PosSessionService : IPosSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<AddPosSessionItemDto> _addItemValidator;
        private readonly ServiceResultHandler _resultHandler;
        private readonly IConfiguration _configuration;
        private readonly IQrCodeService _qrCodeService;
        private readonly IImageStorageService _imageStorageService;
        private readonly ILogger<PosSessionService> _logger;
        private readonly IMyFatoorahClient _myFatoorahClient;


        public PosSessionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<AddPosSessionItemDto> addItemValidator,
            ServiceResultHandler resultHandler,
            IConfiguration configuration,
            IQrCodeService qrCodeService,
            IImageStorageService imageStorageService,
            ILogger<PosSessionService> logger,
            IMyFatoorahClient myFatoorahClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addItemValidator = addItemValidator;
            _resultHandler = resultHandler;
            _configuration = configuration;
            _qrCodeService = qrCodeService;
            _imageStorageService = imageStorageService;
            _logger = logger;
            _myFatoorahClient = myFatoorahClient;
        }

        private decimal GetTaxRate()
        {
            return _configuration.GetValue<decimal>("PosSettings:TaxRate", 0.08m);
        }

        private (decimal subTotal, decimal taxAmount, decimal total) CalculateTotals(PosSession session)
        {
            var subTotal = session.Items.Sum(i => i.UnitPrice * i.Quantity);
            var taxRate = GetTaxRate();
            var taxAmount = subTotal * taxRate;
            var total = subTotal + taxAmount - session.DiscountAmount;
            return (subTotal, taxAmount, total);
        }

        private async Task<PosSession?> GetSessionWithDetailsAsync(int sessionId)
        {
            return await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
        }

        #region Session Management

        public async Task<ServiceResult<PosSessionDto>> CreateSessionAsync(int cashierId, int branchId)
        {
            var cashier = await _unitOfWork.ApplicationUsers.GetByIdAsync(cashierId);
            if (cashier == null || !cashier.IsActive || cashier.IsDeleted)
                return _resultHandler.BadRequest<PosSessionDto>("Invalid cashier");


            var existing = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (existing != null)
                return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(existing));

            var session = new PosSession
            {
                CashierId = cashierId,
                BranchId = branchId,
                Status = PosSessionStatus.Active
            };

            await _unitOfWork.PosSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Created<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> GetSessionAsync(int sessionId)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> GetActiveSessionForCashierAsync(int cashierId)
        {
            var session = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("No active session found");
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<bool>> HoldSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.PosSessions.GetByIdAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<bool>("Session not found");
            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<bool>("Session is not active");

            session.Status = PosSessionStatus.OnHold;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Success<bool>(true);
        }

        public async Task<ServiceResult<PosSessionDto>> ResumeSessionAsync(int sessionId, int cashierId)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.CashierId != cashierId)
                return _resultHandler.BadRequest<PosSessionDto>("Session does not belong to this cashier");
            if (session.Status == PosSessionStatus.Active)
                return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
            if (session.Status != PosSessionStatus.OnHold)
                return _resultHandler.BadRequest<PosSessionDto>($"Cannot resume session with status: {session.Status}");

            var activeSession = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (activeSession != null && activeSession.Id != sessionId)
            {
                activeSession.Status = PosSessionStatus.OnHold;
                _unitOfWork.PosSessions.Update(activeSession);
            }

            session.Status = PosSessionStatus.Active;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<bool>> CancelSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.PosSessions.GetByIdAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<bool>("Session not found");
            if (session.Status == PosSessionStatus.Completed)
                return _resultHandler.BadRequest<bool>("Cannot cancel completed session");

            session.Status = PosSessionStatus.Cancelled;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Success<bool>(true);
        }

        public async Task<ServiceResult<PosSessionDto>> ClearSessionAsync(int sessionId)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            if (session.Items.Any())
            {
                _unitOfWork.PosSessionItems.RemoveRange(session.Items);
                session.CouponCode = null;
                session.DiscountAmount = 0;
                session.CustomerId = null;
                session.CustomerPhone = null;
                session.AmountReceived = 0;
                session.Change = 0;
                _unitOfWork.PosSessions.Update(session);
                await _unitOfWork.SaveChangesAsync();
            }

            session = await GetSessionWithDetailsAsync(sessionId);
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        #endregion

        #region Customer Management

        public async Task<ServiceResult<PosSessionDto>> AssignCustomerToSessionAsync(int sessionId, CreateCustomerDto dto)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            if (string.IsNullOrWhiteSpace(dto.Phone))
                return _resultHandler.BadRequest<PosSessionDto>("Phone number is required");

            var customer = await _unitOfWork.Customers
                .FirstOrDefaultAsync(c => c.Phone == dto.Phone && c.BranchId == session.BranchId);

            if (customer == null)
            {
                customer = new Domin.Entities.Identity.Customer
                {
                    FName = dto.FName,
                    LName = dto.LName,
                    Phone = dto.Phone,
                    Email = dto.Email,
                    Address = dto.Address,
                    Note = dto.Note,
                    BranchId = session.BranchId
                };
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                customer.FName = !string.IsNullOrWhiteSpace(dto.FName) ? dto.FName : customer.FName;
                customer.LName = !string.IsNullOrWhiteSpace(dto.LName) ? dto.LName : customer.LName;
                customer.Email = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : customer.Email;
                customer.Address = !string.IsNullOrWhiteSpace(dto.Address) ? dto.Address : customer.Address;
                customer.Note = !string.IsNullOrWhiteSpace(dto.Note) ? dto.Note : customer.Note;
                _unitOfWork.Customers.Update(customer);
            }

            session.CustomerId = customer.Id;
            session.CustomerPhone = customer.Phone;
            _unitOfWork.PosSessions.Update(session);

            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> AssignCustomerPhoneAsync(int sessionId, string phone)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            session.CustomerPhone = phone;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        #endregion

        #region Item Management

        public async Task<ServiceResult<PosSessionDto>> ScanAndAddItemAsync(ScanPosItemDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return _resultHandler.BadRequest<PosSessionDto>("OTP cannot be empty.");

            var scannedCode = dto.Code.Trim();
            var session = await GetSessionWithDetailsAsync(dto.SessionId);

            if (session == null)
                return _resultHandler.NotFound<PosSessionDto>("POS Session not found.");
            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<PosSessionDto>("POS Session is not active.");

            // ⚠️ UPDATED: Search variant only (product no longer has SKU/Barcode)
            var variant = await _unitOfWork.ProductVariants.FirstOrDefaultAsync(
                v => v.IsActive && (v.Barcode == scannedCode || v.QrCodeValue == scannedCode || v.Sku == scannedCode),
                include: q => q.Include(v => v.Product!).ThenInclude(p => p.Images)
                               .Include(v => v.AttributeValues)
                               .Include(v => v.Stocks),
                ct: ct
            );

            if (variant == null)
                return _resultHandler.NotFound<PosSessionDto>($"No variant found with code: '{scannedCode}'");


            var stock = variant.Stocks.FirstOrDefault(s => s.BranchId == session.BranchId);
            if (stock == null || stock.AvailableQuantity < dto.Quantity)
                return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

            var productName = variant.Product?.Name ?? string.Empty;
            var price = variant.Price;
            var imageUrl = variant.Product?.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? variant.Product?.Images.FirstOrDefault()?.ImageUrl;

            var variantDescription = variant.AttributeValues != null && variant.AttributeValues.Any()
                ? string.Join(", ", variant.AttributeValues.Select(av => av.Value))
                : null;

            var existingItem = session.Items.FirstOrDefault(i =>
                i.ProductId == variant.ProductId && i.ProductVariantId == variant.Id);

            if (existingItem != null)
            {
                var newQty = existingItem.Quantity + dto.Quantity;
                if (stock.AvailableQuantity < newQty)
                    return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock.AvailableQuantity}");

                existingItem.Quantity = newQty;
                _unitOfWork.PosSessionItems.Update(existingItem);
            }
            else
            {
                var newItem = new PosSessionItem
                {
                    PosSessionId = session.Id,
                    ProductId = variant.ProductId,
                    ProductVariantId = variant.Id,
                    ProductName = productName,
                    VariantDescription = variantDescription,
                    ProductImageUrl = imageUrl,
                    UnitPrice = price,
                    Quantity = dto.Quantity
                };
                session.Items.Add(newItem);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            session = await GetSessionWithDetailsAsync(session.Id);
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> AddItemAsync(int sessionId, int productId, int? productVariantId, int quantity)
        {
            var validation = await _addItemValidator.ValidateAsync(
                new AddPosSessionItemDto { ProductId = productId, ProductVariantId = productVariantId, Quantity = quantity });
            if (!validation.IsValid)
                return _resultHandler.BadRequest<PosSessionDto>(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return _resultHandler.BadRequest<PosSessionDto>("Product not found");

            // ⚠️ UPDATED: Variant is required
            if (!productVariantId.HasValue)
                return _resultHandler.BadRequest<PosSessionDto>("Product variant is required.");

            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId.Value);
            if (variant == null || variant.ProductId != productId)
                return _resultHandler.BadRequest<PosSessionDto>("Invalid product variant");
            if (!variant.IsActive)
                return _resultHandler.BadRequest<PosSessionDto>("Product variant is not active");

            var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(productId, productVariantId, session.BranchId);
            if (stock == null || stock.AvailableQuantity < quantity)
                return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

            // ⚠️ UPDATED: Price from variant only
            decimal unitPrice = variant.Price;

            var activeDiscount = product.Discounts?.FirstOrDefault(d => d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);
            if (activeDiscount != null)
            {
                unitPrice = variant.Price - (variant.Price * (activeDiscount.DiscountPercentage / 100m));
            }

            var existingItem = session.Items.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == productVariantId);

            if (existingItem != null)
            {
                var newQty = existingItem.Quantity + quantity;
                if (stock.AvailableQuantity < newQty)
                    return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock for total quantity. Available: {stock.AvailableQuantity}");

                existingItem.Quantity = newQty;
                _unitOfWork.PosSessionItems.Update(existingItem);
            }
            else
            {
                var variantDescription = variant.AttributeValues != null && variant.AttributeValues.Any()
                    ? string.Join(", ", variant.AttributeValues.Select(av => av.Value))
                    : null;

                var item = new PosSessionItem
                {
                    PosSessionId = sessionId,
                    ProductId = productId,
                    ProductVariantId = productVariantId,
                    ProductName = product.Name,
                    VariantDescription = variantDescription,
                    ProductImageUrl = product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = unitPrice,
                    Quantity = quantity
                };
                await _unitOfWork.PosSessionItems.AddAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();
            session = await GetSessionWithDetailsAsync(sessionId);
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> RemoveItemAsync(int sessionId, int itemId)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var item = session.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return _resultHandler.NotFound<PosSessionDto>("Item not found");

            _unitOfWork.PosSessionItems.Remove(item);
            await _unitOfWork.SaveChangesAsync();

            session = await GetSessionWithDetailsAsync(sessionId);
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> UpdateItemQuantityAsync(int sessionId, int itemId, int quantity)
        {
            if (quantity <= 0) return _resultHandler.BadRequest<PosSessionDto>("Quantity must be greater than 0");

            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var item = session.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return _resultHandler.NotFound<PosSessionDto>("Item not found");

            var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(item.ProductId, item.ProductVariantId, session.BranchId);
            if (stock == null || stock.AvailableQuantity < quantity)
                return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

            item.Quantity = quantity;
            _unitOfWork.PosSessionItems.Update(item);
            await _unitOfWork.SaveChangesAsync();

            session = await GetSessionWithDetailsAsync(sessionId);
            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        #endregion

        #region Coupon Management

        public async Task<ServiceResult<PosSessionDto>> ApplyCouponAsync(int sessionId, string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return _resultHandler.BadRequest<PosSessionDto>("Coupon code is required");

            var coupon = await _unitOfWork.Coupons.GetByCodeAsync(couponCode);
            if (coupon == null || !coupon.IsActive)
                return _resultHandler.NotFound<PosSessionDto>("Coupon is invalid or expired");

            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null)
                return _resultHandler.NotFound<PosSessionDto>("Session not found");

            decimal subTotal = session.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal discountAmount = coupon.DiscountType == CouponType.Percentage
                ? (subTotal * coupon.Value / 100)
                : coupon.Value;

            session.CouponCode = coupon.Code;
            session.DiscountAmount = discountAmount;
            session.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> RemoveCouponAsync(int sessionId)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");

            session.CouponCode = null;
            session.DiscountAmount = 0;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success<PosSessionDto>(_mapper.Map<PosSessionDto>(session));
        }

        #endregion

        #region Receipt Preview

        public async Task<ServiceResult<ReceiptPreviewDto>> PreviewReceiptAsync(int sessionId)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<ReceiptPreviewDto>("Session not found");
            if (!session.Items.Any()) return _resultHandler.BadRequest<ReceiptPreviewDto>("Session has no items");

            var (subTotal, taxAmount, total) = CalculateTotals(session);

            var receipt = new ReceiptPreviewDto
            {
                InvoiceNumber = $"#INV-{DateTime.UtcNow:yyyyMMdd}-{session.Id:D4}",
                Date = DateTime.UtcNow,
                CashierName = session.Cashier?.UserName ?? "Unknown",
                BranchName = session.Branch?.Name ?? "Main Store",
                CustomerName = session.Customer != null ? $"{session.Customer.FName} {session.Customer.LName}".Trim() : "Walk-in Customer",
                CustomerPhone = session.CustomerPhone,
                Items = session.Items.Select(i => new ReceiptItemDto
                {
                    Name = i.ProductName,
                    VariantDescription = i.VariantDescription,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.UnitPrice * i.Quantity
                }).ToList(),
                SubTotal = subTotal,
                Discount = session.DiscountAmount,
                Tax = taxAmount,
                Total = total,
                AmountReceived = session.AmountReceived,
                Change = session.Change,
                Payments = new List<SessionPaymentSummaryDto>()
            };

            return _resultHandler.Success<ReceiptPreviewDto>(receipt);
        }

        #endregion

        #region Complete Session

        public async Task<ServiceResult<PosOrderDto>> CompleteSessionAsync(
     int sessionId,
     CompletePosSessionDto dto)
        {
            var session = await GetSessionWithDetailsAsync(sessionId);
            if (session == null)
                return _resultHandler.NotFound<PosOrderDto>("Session not found");

            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<PosOrderDto>("Session is not active");

            if (!session.Items.Any())
                return _resultHandler.BadRequest<PosOrderDto>("Session has no items");

            var (subTotal, taxAmount, total) = CalculateTotals(session);

            var totalPayments = dto.Payments.Sum(p => p.Amount);
            if (dto.Payments.Any() && Math.Abs(totalPayments - total) > 0.001m)
                return _resultHandler.BadRequest<PosOrderDto>(
                    $"Payment amount ({totalPayments}) does not match total ({total})");

            var cashPayment = dto.Payments.FirstOrDefault(p => p.Method == PaymentMethod.Cash);
            if (cashPayment != null && cashPayment.Amount > 0 && dto.AmountReceived < cashPayment.Amount)
                return _resultHandler.BadRequest<PosOrderDto>("Amount received is less than cash payment");

            // ✅ حدد نوع الدفع
            var hasCardPayment = dto.Payments.Any(p => p.Method == PaymentMethod.Visa ||
                                                        p.Method == PaymentMethod.MasterCard ||
                                                        p.Method == PaymentMethod.CreditCard ||
                                                        p.Method == PaymentMethod.DebitCard ||
                                                        p.Method == PaymentMethod.Wallet);

            var cashAmount = cashPayment?.Amount ?? 0;
            var cardAmount = total - cashAmount;

            Order order;
            string? paymentUrl = null;

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // ========== 1. تحقق من Stock ==========
                var stockKeys = session.Items
                    .Select(i => (i.ProductId, i.ProductVariantId))
                    .Distinct()
                    .ToList();

                var stocks = await _unitOfWork.Stocks
                    .GetByProductVariantsAndBranchAsync(stockKeys, session.BranchId);

                foreach (var item in session.Items)
                {
                    var stock = stocks.FirstOrDefault(s =>
                        s.ProductId == item.ProductId &&
                        s.ProductVariantId == item.ProductVariantId);

                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        var variantInfo = item.ProductVariantId.HasValue
                            ? $" (Variant: {item.VariantDescription})"
                            : "";
                        var available = stock?.AvailableQuantity ?? 0;
                        return _resultHandler.BadRequest<PosOrderDto>(
                            $"Insufficient stock for {item.ProductName}{variantInfo}. " +
                            $"Available: {available}, Requested: {item.Quantity}");
                    }
                }

                // ✅ لو كاش فقط → اخصص Stock دلوقتي
                if (!hasCardPayment)
                {
                    foreach (var item in session.Items)
                    {
                        var stock = stocks.First(s =>
                            s.ProductId == item.ProductId &&
                            s.ProductVariantId == item.ProductVariantId);
                        stock.Quantity -= item.Quantity;
                        _unitOfWork.Stocks.Update(stock);
                    }
                }

                // ========== 2. أنشئ الـ Order Items ==========
                var orderItems = session.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    ProductImageUrl = i.ProductImageUrl,
                    VariantDescription = i.VariantDescription,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();

                // ========== 3. حدد الـ Status ==========
                var orderStatus = hasCardPayment ? OrderStatus.PendingPayment : OrderStatus.Completed;
                var primaryMethod = dto.Payments.Count == 1
                    ? dto.Payments[0].Method
                    : PaymentMethod.Cash;

                // ========== 4. أنشئ الـ Order ==========
                order = new Order
                {
                    CashierId = session.CashierId,
                    CustomerId = session.CustomerId,
                    InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{session.Id:D4}",
                    PhoneNumber = session.CustomerPhone ?? string.Empty,
                    PaymentMethod = primaryMethod,
                    Status = orderStatus,
                    Source = OrderSource.Pos,
                    BranchId = session.BranchId,
                    SubTotal = subTotal,
                    TaxAmount = taxAmount,
                    ShippingCost = 0,
                    DiscountAmount = session.DiscountAmount,
                    TotalAmount = total,
                    AmountReceived = dto.AmountReceived,
                    Change = dto.AmountReceived > total ? dto.AmountReceived - total : 0,
                    OrderItems = orderItems,
                };

                // ========== 5. سجّل الـ Transactions ==========
                foreach (var payment in dto.Payments)
                {
                    var isCard = payment.Method == PaymentMethod.Visa ||
                                 payment.Method == PaymentMethod.MasterCard ||
                                 payment.Method == PaymentMethod.CreditCard ||
                                 payment.Method == PaymentMethod.DebitCard ||
                                 payment.Method == PaymentMethod.Wallet;

                    var transaction = new PaymentTransaction
                    {
                        PaymentMethod = payment.Method,
                        Amount = payment.Amount,
                        CurrencyCode = "KWD",
                        Status = isCard ? PaymentStatus.Pending : PaymentStatus.Success,
                        PaidAt = isCard ? null : DateTime.UtcNow, // ✅ nullable
                        Provider = "POS",
                    };
                    order.Transactions.Add(transaction);
                }

                // ========== 6. سجّل الـ Status History ==========
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    Status = orderStatus,
                    EventName = EventName.OrderCreated,
                    Description = hasCardPayment
                        ? "Order created via POS - Awaiting card payment"
                        : "Order created via POS",
                    EventTime = DateTime.UtcNow,
                    PerformedByUserId = session.CashierId
                });

                if (!hasCardPayment)
                {
                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        Status = OrderStatus.Completed,
                        EventName = EventName.PaymentReceived,
                        Description = $"Payment received via {primaryMethod}. Amount: {total:C}",
                        EventTime = DateTime.UtcNow,
                        PerformedByUserId = session.CashierId
                    });

                    order.StatusHistory.Add(new OrderStatusHistory
                    {
                        Status = OrderStatus.Completed,
                        EventName = EventName.ReceiptPrinted,
                        Description = "Receipt printed",
                        EventTime = DateTime.UtcNow,
                        PerformedByUserId = session.CashierId
                    });
                }

                // ========== 7. الكوبون ==========
                if (!string.IsNullOrEmpty(session.CouponCode))
                {
                    var coupon = await _unitOfWork.Coupons
                        .FirstOrDefaultAsync(c => c.Code == session.CouponCode);
                    if (coupon != null)
                    {
                        coupon.UsedCount++;
                        _unitOfWork.Coupons.Update(coupon);
                    }
                }

                // ========== 8. احفظ الـ Order ==========
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                // ========== 9. اربط الـ Session بالـ Order ==========
                session.OrderId = order.Id;
                session.AmountReceived = dto.AmountReceived;
                session.Change = order.Change;

                if (!hasCardPayment)
                {
                    session.Status = PosSessionStatus.Completed;
                }
                else
                {
                    session.Status = PosSessionStatus.PendingPayment;
                }

                _unitOfWork.PosSessions.Update(session);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // ========== 10. لو فيه كارد → استدعي MyFatoorah (برة الترانزاكشن!) ==========
            if (hasCardPayment)
            {
                try
                {
                    var cardPayment = dto.Payments.First(p =>
                        p.Method == PaymentMethod.Visa ||
                        p.Method == PaymentMethod.MasterCard ||
                        p.Method == PaymentMethod.CreditCard ||
                        p.Method == PaymentMethod.DebitCard ||
                        p.Method == PaymentMethod.Wallet);

                    // ✅ استخدم القيم من appsettings أو من Session
                    var executeRequest = new ExecutePaymentRequestModel
                    {
                        InvoiceValue = Math.Round(cardAmount, 3),

                        PaymentMethodId = GetMyFatoorahPaymentMethodId(cardPayment.Method),

                        CustomerName = session.Customer?.FName ?? "POS Customer",
                        CustomerEmail = session.Customer?.Email ?? "pos@onpoint.store",
                        CustomerMobile = session.CustomerPhone ?? session.Customer?.Phone ?? "",
                        CallBackUrl = _configuration["MyFatoorah:CallbackUrl"] ?? "https://yourapp.com/api/payments/callback",
                        ErrorUrl = _configuration["MyFatoorah:ErrorUrl"] ?? "https://yourapp.com/payment/error",
                        CustomerReference = order.Id.ToString(),
                        Language = "AR",
                        DisplayCurrencyIso = "KWD",
                    };

                    var myFatoorahResult = await _myFatoorahClient
                        .ExecutePaymentAsync(executeRequest);

                    // ✅ استخدم GatewayTransactionId الموجود
                    var cardTransaction = order.Transactions
                        .FirstOrDefault(t => t.PaymentMethod == cardPayment.Method);
                    if (cardTransaction == null)
                    {
                        _logger.LogError("Pending card transaction not found for Order {OrderId}", order.Id);
                        return _resultHandler.BadRequest<PosOrderDto>("Internal error: transaction not found.");
                    }
                    cardTransaction.GatewayTransactionId = myFatoorahResult.InvoiceId;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync();

                    paymentUrl = myFatoorahResult.PaymentUrl;

                    _logger.LogInformation(
                        "MyFatoorah payment initiated for Order {OrderId}, Invoice {InvoiceId}",
                        order.Id, myFatoorahResult.InvoiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MyFatoorah payment initiation failed for Order {OrderId}", order.Id);

                    await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
                    try
                    {
                        order.Status = OrderStatus.PaymentFailed;
                        _unitOfWork.Orders.Update(order);

                        session.Status = PosSessionStatus.PaymentFailed;
                        _unitOfWork.PosSessions.Update(session);

                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }

                    return _resultHandler.BadRequest<PosOrderDto>(
                        "Failed to initiate card payment. Please try again.");
                }
            }

            // ========== 11. QR Code ==========
            try
            {
                var qrValue = $"https://yourapp.com/invoice/{order.Id}";
                var qrBytes = _qrCodeService.GenerateImage(qrValue);
                using var qrStream = new MemoryStream(qrBytes);
                var fileName = $"invoice-qr-{order.Id}-{Guid.NewGuid()}.png";
                var uploadResult = await _imageStorageService.UploadImageAsync(qrStream, fileName);

                if (uploadResult.Succeeded)
                {
                    order.QRCode = uploadResult.Data;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR generation failed for Order {OrderId}", order.Id);
            }

            // ========== 12. رجّع الـ DTO ==========
            var resultDto = _mapper.Map<PosOrderDto>(order);
            resultDto.PaymentUrl = paymentUrl;

            if (!string.IsNullOrEmpty(paymentUrl))
            {
                return _resultHandler.Success(resultDto, "Payment required to complete the order.");
            }

            return _resultHandler.Created(resultDto);
        }

        #endregion
        private int GetMyFatoorahPaymentMethodId(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.KNet => 1,
                PaymentMethod.Visa => 2,
                PaymentMethod.MasterCard => 2,
                PaymentMethod.CreditCard => 2,
                PaymentMethod.DebitCard => 2,

                _ => 2
            };
        }
    }
}