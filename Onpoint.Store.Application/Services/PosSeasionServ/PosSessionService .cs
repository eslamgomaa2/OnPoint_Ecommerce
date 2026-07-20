using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Pos;
using Onpoint.Store.Application.DTOs.PosSession;
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

        public PosSessionService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AddPosSessionItemDto> addItemValidator, ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addItemValidator = addItemValidator;
            _resultHandler = resultHandler;
        }
        public async Task<ServiceResult<PosSessionDto>> ScanAndAddItemAsync(ScanPosItemDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Code))
                return _resultHandler.BadRequest<PosSessionDto>("Code cannot be empty.");

            var scannedCode = dto.Code.Trim();

            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(dto.SessionId);

            if (session == null)
                return _resultHandler.NotFound<PosSessionDto>("POS Session not found.");

            if (session.Status != PosSessionStatus.Active)
                return _resultHandler.BadRequest<PosSessionDto>("POS Session is not active.");

            var variant = await _unitOfWork.ProductVariants.FirstOrDefaultAsync(
                v => v.IsActive && (v.Barcode == scannedCode || v.QrCodeValue == scannedCode),
                include: q => q.Include(v => v.Product!).ThenInclude(p => p.Images)
                               .Include(v => v.AttributeValues),
                ct: ct
            );

            int productId;
            int? variantId = null;
            string productName;
            string? variantDescription = null;
            string? imageUrl = null;
            decimal price;

            if (variant != null)
            {
                productId = variant.ProductId;
                variantId = variant.Id;
                productName = variant.Product?.Name ?? string.Empty;
                price = variant.Price > 0 ? variant.Price : (variant.Product?.Price ?? 0);
                imageUrl = variant.Product?.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? variant.Product?.Images.FirstOrDefault()?.ImageUrl;


                if (variant.AttributeValues != null && variant.AttributeValues.Any())
                {
                    variantDescription = string.Join(", ", variant.AttributeValues.Select(av => av.Value));
                }
            }
            else
            {
                var product = await _unitOfWork.Products.FirstOrDefaultAsync(
                    p => (p.Barcode == scannedCode || p.QrCodeValue == scannedCode),
                    include: q => q.Include(p => p.Images),
                    ct: ct
                );

                if (product == null)
                    return _resultHandler.NotFound<PosSessionDto>($"No product or variant found with code: '{scannedCode}'");

                productId = product.Id;
                productName = product.Name;
                price = product.Price;
                imageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? product.Images.FirstOrDefault()?.ImageUrl;
            }

            var existingItem = session.Items.FirstOrDefault(i =>
                i.ProductId == productId && i.ProductVariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                _unitOfWork.PosSessionItems.Update(existingItem);
            }
            else
            {
                var newItem = new PosSessionItem
                {
                    PosSessionId = session.Id,
                    ProductId = productId,
                    ProductVariantId = variantId,
                    ProductName = productName,
                    VariantDescription = variantDescription,
                    ProductImageUrl = imageUrl,
                    UnitPrice = price,
                    Quantity = dto.Quantity
                };

                session.Items.Add(newItem);
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }
        public async Task<ServiceResult<PosSessionDto>> CreateSessionAsync(int cashierId)
        {
            var cashier = await _unitOfWork.ApplicationUsers.GetByIdAsync(cashierId);
            if (cashier == null || !cashier.IsActive || cashier.IsDeleted) return _resultHandler.BadRequest<PosSessionDto>("Invalid cashier");
            if (!cashier.BranchId.HasValue) return _resultHandler.BadRequest<PosSessionDto>("Cashier not assigned to branch");

            var existing = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (existing != null) return _resultHandler.Success(_mapper.Map<PosSessionDto>(existing));

            var session = new PosSession { CashierId = cashierId, BranchId = cashier.BranchId.Value, Status = PosSessionStatus.Active };
            await _unitOfWork.PosSessions.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Created(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> GetSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> GetActiveSessionForCashierAsync(int cashierId)
        {
            var session = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("No active session found");
            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> AddItemAsync(int sessionId, int productId, int? productVariantId, int quantity)
        {
            // 1. Validation
            var validation = await _addItemValidator.ValidateAsync(new AddPosSessionItemDto { ProductId = productId, ProductVariantId = productVariantId, Quantity = quantity });
            if (!validation.IsValid)
                return _resultHandler.BadRequest<PosSessionDto>(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return _resultHandler.BadRequest<PosSessionDto>("Product not found");

            ProductVariant? variant = null;
            if (productVariantId.HasValue)
            {
                variant = await _unitOfWork.ProductVariants.GetByIdAsync(productVariantId.Value);
                if (variant == null || variant.ProductId != productId)
                    return _resultHandler.BadRequest<PosSessionDto>("Invalid product variant");

                if (!variant.IsActive)
                    return _resultHandler.BadRequest<PosSessionDto>("Product variant is not active");
            }
            else
            {
                bool hasActiveVariants = await _unitOfWork.ProductVariants.AnyAsync(v => v.ProductId == productId && v.IsActive);
                if (hasActiveVariants)
                {
                    return _resultHandler.BadRequest<PosSessionDto>("Please select a product variant.");
                }
            }

            var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(productId, productVariantId, session.BranchId);
            if (stock == null || stock.AvailableQuantity < quantity)
                return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

            decimal basePrice = variant?.Price ?? product.Price;
            decimal unitPrice = basePrice;

            var activeDiscount = product.Discounts?.FirstOrDefault(d => d.IsActive && d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow);
            if (activeDiscount != null)
            {
                unitPrice = basePrice - (basePrice * (activeDiscount.DiscountPercentage / 100m));
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
                var item = new PosSessionItem
                {
                    PosSessionId = sessionId,
                    ProductId = productId,
                    ProductVariantId = productVariantId,
                    ProductName = product.Name,
                    ProductImageUrl = product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = unitPrice,
                    Quantity = quantity
                };

                await _unitOfWork.PosSessionItems.AddAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();

            session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> RemoveItemAsync(int sessionId, int itemId)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var item = session.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return _resultHandler.NotFound<PosSessionDto>("Item not found");

            _unitOfWork.PosSessionItems.Remove(item);
            await _unitOfWork.SaveChangesAsync();

            session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> UpdateItemQuantityAsync(int sessionId, int itemId, int quantity)
        {
            if (quantity <= 0) return _resultHandler.BadRequest<PosSessionDto>("Quantity must be greater than 0");

            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var item = session.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return _resultHandler.NotFound<PosSessionDto>("Item not found");

            var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(item.ProductId, item.ProductVariantId, session.BranchId);
            if (stock == null || stock.AvailableQuantity < quantity) return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

            item.Quantity = quantity;
            _unitOfWork.PosSessionItems.Update(item);
            await _unitOfWork.SaveChangesAsync();

            session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> ApplyCouponAsync(int sessionId, string couponCode)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
            if (coupon == null) return _resultHandler.BadRequest<PosSessionDto>("Invalid coupon code");

            var subTotal = session.Items.Sum(i => i.UnitPrice * i.Quantity);
            bool valid = coupon.IsActive && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow && coupon.UsedCount < coupon.MaxUses && subTotal >= coupon.MinOrderAmount;
            if (!valid) return _resultHandler.BadRequest<PosSessionDto>("Coupon is not valid");

            var discount = coupon.DiscountType == CouponType.Percentage ? (subTotal * coupon.Value) / 100 : coupon.Value;
            if (discount > subTotal) discount = subTotal;

            session.CouponCode = couponCode;
            session.DiscountAmount = discount;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosSessionDto>> RemoveCouponAsync(int sessionId)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");

            session.CouponCode = null;
            session.DiscountAmount = 0;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<PosOrderDto>> CompleteSessionAsync(int sessionId, PaymentMethod paymentMethod, string customerPhone)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosOrderDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosOrderDto>("Session is not active");
            if (!session.Items.Any()) return _resultHandler.BadRequest<PosOrderDto>("Session has no items");

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var stockKeys = session.Items.Select(i => (i.ProductId, i.ProductVariantId)).Distinct().ToList();
                var stocks = await _unitOfWork.Stocks.GetByProductVariantsAndBranchAsync(stockKeys, session.BranchId);

                foreach (var item in session.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId && s.ProductVariantId == item.ProductVariantId);

                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        var variantInfo = item.ProductVariantId.HasValue ? $" (Variant: {item.VariantDescription})" : "";
                        var available = stock?.AvailableQuantity ?? 0;

                        return _resultHandler.BadRequest<PosOrderDto>(
                            $"Insufficient stock for {item.ProductName}{variantInfo}. Available: {available}, Requested: {item.Quantity}"
                        );
                    }

                    stock.Quantity -= item.Quantity;
                    _unitOfWork.Stocks.Update(stock);
                }

                var subTotal = session.Items.Sum(i => i.UnitPrice * i.Quantity);

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

                var order = new Order
                {
                    UserId = session.CashierId,
                    AddressId = null,
                    OrderNumber = $"POS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
                    PhoneNumber = customerPhone,
                    PaymentMethod = paymentMethod,
                    Status = OrderStatus.Delivered,
                    Source = OrderSource.Pos,
                    BranchId = session.BranchId,
                    SubTotal = subTotal,
                    ShippingCost = 0,
                    DiscountAmount = session.DiscountAmount,
                    TotalAmount = subTotal - session.DiscountAmount,
                    OrderItems = orderItems
                };

                if (!string.IsNullOrEmpty(session.CouponCode))
                {
                    var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == session.CouponCode);
                    if (coupon != null)
                    {
                        coupon.UsedCount++;
                        _unitOfWork.Coupons.Update(coupon);
                    }
                }

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                session.Status = PosSessionStatus.Completed;
                session.OrderId = order.Id;
                _unitOfWork.PosSessions.Update(session);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return _resultHandler.Created(_mapper.Map<PosOrderDto>(order));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        public async Task<ServiceResult<bool>> HoldSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.PosSessions.GetByIdAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<bool>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<bool>("Session is not active");

            session.Status = PosSessionStatus.OnHold;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Success(true);
        }

        public async Task<ServiceResult<PosSessionDto>> ResumeSessionAsync(int sessionId, int cashierId)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.CashierId != cashierId) return _resultHandler.BadRequest<PosSessionDto>("Session does not belong to this cashier");
            if (session.Status == PosSessionStatus.Active) return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
            if (session.Status != PosSessionStatus.OnHold) return _resultHandler.BadRequest<PosSessionDto>($"Cannot resume session with status: {session.Status}");

            var activeSession = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (activeSession != null && activeSession.Id != sessionId) { activeSession.Status = PosSessionStatus.OnHold; _unitOfWork.PosSessions.Update(activeSession); }

            session.Status = PosSessionStatus.Active;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));
        }

        public async Task<ServiceResult<bool>> CancelSessionAsync(int sessionId)
        {
            var session = await _unitOfWork.PosSessions.GetByIdAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<bool>("Session not found");
            if (session.Status == PosSessionStatus.Completed) return _resultHandler.BadRequest<bool>("Cannot cancel completed session");

            session.Status = PosSessionStatus.Cancelled;
            _unitOfWork.PosSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();
            return _resultHandler.Success(true);
        }
    }
}