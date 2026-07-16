using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.OrderServ
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly ServiceResultHandler _serviceResultHandler;
        private readonly IConfiguration _configuration;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateOrderDto> createValidator,
            ServiceResultHandler serviceResultHandler,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
            _serviceResultHandler = serviceResultHandler;
            _configuration = configuration;
        }

        public async Task<ServiceResult<OrderDto>> CheckoutAsync(int userId, CreateOrderDto dto)
        {
            await _createValidator.ValidateAndThrowAsync(dto);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId);

                if (cart == null || !cart.Items.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return _serviceResultHandler.BadRequest<OrderDto>("Cart is empty");
                }


                var defaultBranch = await _unitOfWork.Branches
                    .FirstOrDefaultAsync(b => b.IsDefault && b.IsActive);

                if (defaultBranch == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return _serviceResultHandler.BadRequest<OrderDto>("Default online branch is not configured");
                }

                var productIds = cart.Items.Select(i => i.ProductId).ToList();
                var products = await _unitOfWork.Products.GetByIdsAsync(productIds);
                var stocks = await _unitOfWork.Stocks.GetByProductIdsAsync(productIds, defaultBranch.Id);

                foreach (var item in cart.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return _serviceResultHandler.BadRequest<OrderDto>($"Product {item.ProductId} not found");
                    }

                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);
                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return _serviceResultHandler.BadRequest<OrderDto>($"Not enough stock for {product.Name}");
                    }
                }

                decimal discountAmount = 0;
                Coupon? coupon = null;

                if (!string.IsNullOrWhiteSpace(cart.AppliedCouponCode))
                {
                    coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode);

                    bool couponStillValid = coupon != null
                        && coupon.IsActive
                        && coupon.StartDate <= DateTime.Now
                        && coupon.EndDate >= DateTime.Now
                        && coupon.UsedCount < coupon.MaxUses;

                    if (!couponStillValid)
                    {
                        coupon = null;
                        cart.AppliedCouponCode = null;
                        cart.DiscountAmount = 0;
                    }
                }

                var order = new Order
                {
                    UserId = userId,
                    AddressId = dto.AddressId,
                    PhoneNumber = dto.PhoneNumber,
                    PaymentMethod = dto.PaymentMethod,
                    OrderNumber = GenerateOrderNumber(),
                    Status = OrderStatus.Pending,
                    BranchId = defaultBranch.Id,
                    Source = OrderSource.Online,
                    OrderItems = new List<OrderItem>()
                };

                decimal subTotal = 0;
                foreach (var item in cart.Items)
                {
                    var product = products.First(p => p.Id == item.ProductId);

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductImageUrl = product.Images.FirstOrDefault()?.ImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };

                    order.OrderItems.Add(orderItem);
                    subTotal += orderItem.TotalPrice;
                }

                order.SubTotal = subTotal;
                order.ShippingCost = GetShippingCost();

                if (coupon != null)
                {
                    if (order.SubTotal < coupon.MinOrderAmount)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return _serviceResultHandler.BadRequest<OrderDto>(
                            $"Minimum order amount for this coupon is {coupon.MinOrderAmount}");
                    }

                    discountAmount = coupon.DiscountType == CouponType.Percentage
                        ? (order.SubTotal * coupon.Value) / 100
                        : coupon.Value;

                    if (discountAmount > order.SubTotal)
                        discountAmount = order.SubTotal;

                    order.DiscountAmount = discountAmount;
                }

                order.TotalAmount = order.SubTotal + order.ShippingCost - order.DiscountAmount;

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                if (dto.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    await FinalizeOrderAsync(order, cart, coupon, ct: default);
                    order.Status = OrderStatus.Processing;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                var orderDto = _mapper.Map<OrderDto>(order);
                return _serviceResultHandler.Created<OrderDto>(orderDto);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task FinalizeOrderAsync(Order order, Cart? cart, Coupon? coupon, CancellationToken ct = default)
        {
            var branchId = order.BranchId ?? await GetDefaultBranchIdAsync(ct);

            var productIds = order.OrderItems.Select(i => i.ProductId).ToList();
            var stocks = await _unitOfWork.Stocks.GetByProductIdsAsync(productIds, branchId, ct);

            foreach (var item in order.OrderItems)
            {
                var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId);

                if (stock is null)
                    throw new InvalidOperationException($"Stock record not found for product {item.ProductName}");

                if (stock.AvailableQuantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for {item.ProductName}. Available: {stock.AvailableQuantity}, Requested: {item.Quantity}");

                stock.Quantity -= item.Quantity;
                _unitOfWork.Stocks.Update(stock);
            }

            if (coupon != null)
            {
                coupon.UsedCount++;
                _unitOfWork.Coupons.Update(coupon);
            }

            if (cart != null)
                _unitOfWork.Carts.Remove(cart);
        }

        public async Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetUserOrders(userId);
            if (orders == null || !orders.Any())
                return _serviceResultHandler.NotFound<IEnumerable<OrderDto>>("No orders found for this user");

            var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return _serviceResultHandler.Success<IEnumerable<OrderDto>>(ordersDto);
        }

        public async Task<ServiceResult<bool>> UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return _serviceResultHandler.NotFound<bool>("Order not found");

            order.Status = status;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return _serviceResultHandler.Success<bool>(true);
        }

        private async Task<int> GetDefaultBranchIdAsync(CancellationToken ct = default)
        {
            var defaultBranch = await _unitOfWork.Branches
                .FirstOrDefaultAsync(b => b.IsDefault && b.IsActive, ct);

            if (defaultBranch == null)
                throw new InvalidOperationException("Default online branch is not configured");

            return defaultBranch.Id;
        }

        private decimal GetShippingCost()
        {
            return _configuration.GetValue<decimal>("CartSettings:FixedShippingCost", 10.0m);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";
        }
    }
}