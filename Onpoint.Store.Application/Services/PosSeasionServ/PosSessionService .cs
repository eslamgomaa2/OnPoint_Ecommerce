using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.PosSession;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.PosServ
{
    public class PosSessionService : IPosSessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<AddPosSessionItemDto> _addItemValidator;
        private readonly ServiceResultHandler _resultHandler;

        public PosSessionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<AddPosSessionItemDto> addItemValidator,
            ServiceResultHandler resultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addItemValidator = addItemValidator;
            _resultHandler = resultHandler;
        }

        public async Task<ServiceResult<PosSessionDto>> CreateSessionAsync(int cashierId)
        {
            var cashier = await _unitOfWork.ApplicationUsers.GetByIdAsync(cashierId);
            if (cashier == null || !cashier.IsActive || cashier.IsDeleted)
                return _resultHandler.BadRequest<PosSessionDto>("Invalid cashier");
            if (!cashier.BranchId.HasValue)
                return _resultHandler.BadRequest<PosSessionDto>("Cashier not assigned to branch");

            var existing = await _unitOfWork.PosSessions.GetActiveSessionByCashierAsync(cashierId);
            if (existing != null)
                return _resultHandler.Success(_mapper.Map<PosSessionDto>(existing));

            var session = new PosSession
            {
                CashierId = cashierId,
                BranchId = cashier.BranchId.Value,
                Status = PosSessionStatus.Active
            };

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

        public async Task<ServiceResult<PosSessionDto>> AddItemAsync(int sessionId, int productId, int quantity)
        {
            await _addItemValidator.ValidateAsync(new AddPosSessionItemDto { ProductId = productId, Quantity = quantity });

            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosSessionDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosSessionDto>("Session is not active");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return _resultHandler.BadRequest<PosSessionDto>("Product not found");

            var stock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(productId, session.BranchId);
            if (stock == null || stock.AvailableQuantity < quantity)
                return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

            var existingItem = session.Items.FirstOrDefault(i => i.ProductId == productId);
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
                    ProductName = product.Name,
                    ProductImageUrl = product.Images.FirstOrDefault()?.ImageUrl,
                    UnitPrice = product.Price,
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

            var stock = await _unitOfWork.Stocks.GetByProductAndBranchAsync(item.ProductId, session.BranchId);
            if (stock == null || stock.AvailableQuantity < quantity)
                return _resultHandler.BadRequest<PosSessionDto>($"Insufficient stock. Available: {stock?.AvailableQuantity ?? 0}");

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
            bool valid = coupon.IsActive && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow
                && coupon.UsedCount < coupon.MaxUses && subTotal >= coupon.MinOrderAmount;

            if (!valid) return _resultHandler.BadRequest<PosSessionDto>("Coupon is not valid");

            var discount = coupon.DiscountType == CouponType.Percentage
                ? (subTotal * coupon.Value) / 100
                : coupon.Value;
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

        public async Task<ServiceResult<PosOrderDto>> CompleteSessionAsync(int sessionId, PaymentMethod paymentMethod)
        {
            var session = await _unitOfWork.PosSessions.GetSessionWithItemsAsync(sessionId);
            if (session == null) return _resultHandler.NotFound<PosOrderDto>("Session not found");
            if (session.Status != PosSessionStatus.Active) return _resultHandler.BadRequest<PosOrderDto>("Session is not active");
            if (!session.Items.Any()) return _resultHandler.BadRequest<PosOrderDto>("Session has no items");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var subTotal = session.Items.Sum(i => i.UnitPrice * i.Quantity);
                var orderItems = session.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductImageUrl = i.ProductImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList();

                var order = new Order
                {
                    UserId = session.CashierId,
                    OrderNumber = $"POS-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}",
                    PhoneNumber = session.CustomerPhone ?? string.Empty,
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

                var productIds = session.Items.Select(i => i.ProductId).ToList();
                var stocks = await _unitOfWork.Stocks.GetByProductIdsAsync(productIds, session.BranchId);

                foreach (var item in session.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);
                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for {item.ProductName}");

                    stock.Quantity -= item.Quantity;
                    _unitOfWork.Stocks.Update(stock);
                }

                if (!string.IsNullOrEmpty(session.CouponCode))
                {
                    var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == session.CouponCode);
                    if (coupon != null) { coupon.UsedCount++; _unitOfWork.Coupons.Update(coupon); }
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
            if (session == null)
                return _resultHandler.NotFound<PosSessionDto>("Session not found");


            if (session.CashierId != cashierId)
                return _resultHandler.BadRequest<PosSessionDto>("Session does not belong to this cashier");


            if (session.Status == PosSessionStatus.Active)
                return _resultHandler.Success(_mapper.Map<PosSessionDto>(session));

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