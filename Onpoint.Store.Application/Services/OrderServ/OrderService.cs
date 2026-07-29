using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Onpoint.Store.Application.DTOs.Order;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Entities.Sales;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using System.Data;

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

        public async Task<ServiceResult<OrderDto>> CheckoutAsync(int userId, CreateOrderDto dto, CancellationToken ct = default)
        {
            await _createValidator.ValidateAndThrowAsync(dto, cancellationToken: ct);

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId, ct);
                if (cart == null || !cart.Items.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return _serviceResultHandler.BadRequest<OrderDto>("Cart is empty.");
                }

                var defaultBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(b => b.IsDefault && b.IsActive, ct);
                if (defaultBranch == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return _serviceResultHandler.BadRequest<OrderDto>("Default online branch is not configured.");
                }

                var stockKeys = cart.Items.Select(i => (i.ProductId, i.ProductVariantId)).Distinct().ToList();
                var stocks = await _unitOfWork.Stocks.GetByProductVariantsAndBranchAsync(stockKeys, defaultBranch.Id, ct);

                foreach (var item in cart.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId && s.ProductVariantId == item.ProductVariantId);
                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return _serviceResultHandler.BadRequest<OrderDto>($"Not enough stock for {item.Product?.Name ?? "product"}.");
                    }
                }

                decimal discountAmount = 0;
                Coupon? coupon = null;

                if (!string.IsNullOrWhiteSpace(cart.AppliedCouponCode))
                {
                    coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode, ct);

                    bool couponStillValid = coupon != null && coupon.IsActive && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow && coupon.UsedCount < coupon.MaxUses;

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
                    Status = OrderStatus.Pending,
                    BranchId = defaultBranch.Id,
                    Source = OrderSource.Online,
                    CouponCode = coupon?.Code,
                    OrderItems = new List<OrderItem>()
                };

                decimal subTotal = 0;
                foreach (var item in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        ProductName = item.Product?.Name ?? string.Empty,
                        ProductImageUrl = item.Product?.Images.FirstOrDefault()?.ImageUrl,
                        VariantDescription = item.ProductVariant != null ? string.Join(", ", item.ProductVariant.AttributeValues.Select(av => av.Value)) : null,
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
                        return _serviceResultHandler.BadRequest<OrderDto>($"Minimum order amount for this coupon is {coupon.MinOrderAmount}.");
                    }

                    discountAmount = coupon.DiscountType == CouponType.Percentage ? (order.SubTotal * coupon.Value) / 100 : coupon.Value;
                    if (discountAmount > order.SubTotal) discountAmount = order.SubTotal;
                    order.DiscountAmount = discountAmount;
                }

                order.TotalAmount = order.SubTotal + order.ShippingCost - order.DiscountAmount;

                await _unitOfWork.Orders.AddAsync(order, ct);

                if (dto.PaymentMethod == PaymentMethod.Cash)
                {
                    await FinalizeOrderAsync(order, cart, coupon, ct);
                    order.Status = OrderStatus.Pending;
                }

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync();

                var orderDto = _mapper.Map<OrderDto>(order);
                return _serviceResultHandler.Created(orderDto);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<UserOrderDetailsResponseDto>> GetOrderItemsAsync(int orderId, int userId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetOrderItemsForUserAsync(orderId, userId, ct);
            if (order == null)
                return _serviceResultHandler.NotFound<UserOrderDetailsResponseDto>("Order not found.");

            var items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductVariantId = oi.ProductVariantId,
                ProductName = oi.Product?.Name ?? oi.ProductName ?? "Unknown Product",
                ProductImageUrl = oi.Product?.Images?
                    .Where(i => i.IsPrimary)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault()
                    ?? oi.Product?.Images?
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                    ?? oi.ProductImageUrl,
                VariantAttributes = oi.ProductVariant?.AttributeValues?
                    .Select(av => new VariantAttributeDto
                    {
                        AttributeName = av.ProductAttribute?.Name ?? "Attribute",
                        AttributeValue = av.Value
                    })
                    .ToList() ?? new List<VariantAttributeDto>(),
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
            }).ToList();

            var summary = new UserOrderSummaryDto
            {
                SubTotal = order.SubTotal,
                CouponCode = order.CouponCode,
                CouponDiscountPercentage = order.SubTotal > 0
                    ? Math.Round((order.DiscountAmount / order.SubTotal) * 100, 2)
                    : null,
                DiscountAmount = order.DiscountAmount,
                ShippingCost = order.ShippingCost,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod.ToString()
            };

            var response = new UserOrderDetailsResponseDto
            {
                Items = items,
                Summary = summary
            };

            return _serviceResultHandler.Success(response);
        }
        public async Task<ServiceResult<bool>> ProcessPaymentSuccessAsync(int orderId, string transactionId, CancellationToken ct = default)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
                if (order == null) return _serviceResultHandler.NotFound<bool>("Order not found.");
                if (order.Status != OrderStatus.Pending) return _serviceResultHandler.BadRequest<bool>("Order is not pending.");

                if (order.Transactions.Any(t => t.Status == PaymentStatus.Success))
                    return _serviceResultHandler.Success(true);

                Coupon? coupon = null;
                if (!string.IsNullOrEmpty(order.CouponCode))
                    coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode, ct);

                var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(order.CustomerId.Value, ct);

                await FinalizeOrderAsync(order, cart, coupon, ct);

                order.Status = OrderStatus.Pending;
                _unitOfWork.Orders.Update(order);

                order.Transactions.Add(new PaymentTransaction
                {
                    OrderId = orderId,
                    GatewayTransactionId = transactionId,
                    Amount = order.TotalAmount,
                    Status = PaymentStatus.Success,
                    PaymentMethod = order.PaymentMethod,
                    PaidAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync();

                return _serviceResultHandler.Success(true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<bool>> CancelOrderAsync(int orderId, CancellationToken ct = default)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
                if (order == null) return _serviceResultHandler.NotFound<bool>("Order not found.");

                if (order.Status == OrderStatus.Completed)
                    return _serviceResultHandler.BadRequest<bool>("Cannot cancel delivered order.");

                if (order.Status == OrderStatus.Pending)
                {
                    var stockKeys = order.OrderItems.Select(i => (i.ProductId, i.ProductVariantId)).Distinct().ToList();
                    var stocks = await _unitOfWork.Stocks.GetByProductVariantsAndBranchAsync(stockKeys, order.BranchId, ct);

                    foreach (var item in order.OrderItems)
                    {
                        var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId && s.ProductVariantId == item.ProductVariantId);
                        if (stock != null)
                        {
                            stock.Quantity += item.Quantity;
                            _unitOfWork.Stocks.Update(stock);
                        }
                    }

                    if (!string.IsNullOrEmpty(order.CouponCode))
                    {
                        var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode, ct);
                        if (coupon != null && coupon.UsedCount > 0)
                        {
                            coupon.UsedCount--;
                            _unitOfWork.Coupons.Update(coupon);
                        }
                    }
                }

                order.Status = OrderStatus.Refunded;
                _unitOfWork.Orders.Update(order);

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync();

                return _serviceResultHandler.Success(true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<bool>> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, CancellationToken ct = default)
        {
            if (newStatus == OrderStatus.Refunded)
            {
                return await CancelOrderAsync(orderId, ct);
            }

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
            if (order == null) return _serviceResultHandler.NotFound<bool>("Order not found.");

            var validTransition = (order.Status, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Pending) => true,
                (OrderStatus.Pending, OrderStatus.Completed) => true,
                _ => false
            };

            if (!validTransition)
                return _serviceResultHandler.BadRequest<bool>($"Invalid status transition from {order.Status} to {newStatus}.");

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            return _serviceResultHandler.Success(true);
        }

        public async Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId, CancellationToken ct = default)
        {
            var orders = await _unitOfWork.Orders.GetUserOrders(userId);
            if (orders == null || !orders.Any())
                return _serviceResultHandler.NotFound<IEnumerable<OrderDto>>("No orders found for this user.");

            var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return _serviceResultHandler.Success(ordersDto);
        }

        public async Task<ServiceResult<OrderDto>> GetOrderAsync(int orderId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, ct);
            if (order == null)
                return _serviceResultHandler.NotFound<OrderDto>("Order not found.");

            return _serviceResultHandler.Success(_mapper.Map<OrderDto>(order));
        }

        public async Task FinalizeOrderAsync(Order order, Cart? cart, Coupon? coupon, CancellationToken ct = default)
        {
            var branchId = order.BranchId;

            var stockKeys = order.OrderItems
                .Select(i => (i.ProductId, i.ProductVariantId))
                .Distinct()
                .ToList();

            var stocks = await _unitOfWork.Stocks.GetByProductVariantsAndBranchAsync(stockKeys, branchId, ct);

            foreach (var item in order.OrderItems)
            {
                var stock = stocks.FirstOrDefault(s =>
                    s.ProductId == item.ProductId && s.ProductVariantId == item.ProductVariantId);

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
                if (coupon.UsedCount >= coupon.MaxUses)
                    throw new InvalidOperationException("This coupon has reached its maximum usage limit.");

                coupon.UsedCount++;
                _unitOfWork.Coupons.Update(coupon);
            }

            if (cart != null)
                _unitOfWork.Carts.Remove(cart);
        }

        public async Task<ServiceResult<PagedResult<OrderListItemDto>>> GetDashboardPagedAsync(GetOrdersQueryDto query, int? branchId, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Orders.GetOrdersPagedAsync(
                query.PageNumber, query.PageSize, query.SortBy, query.Descending, branchId, ct);

            var list = items.Select(o => new OrderListItemDto
            {
                Id = o.Id,
                OrderId = $"ORD-00{o.Id}",
                OrderDate = o.CreatedAt,
                Status = o.Status,
                TotalAmount = o.TotalAmount
            }).ToList();

            var result = PagedResult<OrderListItemDto>.Create(list, totalCount, query.PageNumber, query.PageSize);
            return _serviceResultHandler.Success(result);
        }

        public async Task<ServiceResult<DashBoardSummaryDto>> GetDashboardSummaryAsync(int? branchId, CancellationToken ct = default)
        {
            var total = await _unitOfWork.Orders.GetTotalOrdersCountAsync(branchId, ct);
            var totalProducts = await _unitOfWork.Stocks.GetProductsCountByBranchAsync(branchId, ct);
            var missingQuantity = await _unitOfWork.Stocks.GetMissingQuantityCountByBranchAsync(branchId, ct);

            var dto = new DashBoardSummaryDto
            {
                TotalOrders = total,
                NumberOfProducts = totalProducts,
                MissingQuantity = missingQuantity
            };

            return _serviceResultHandler.Success(dto);
        }


        public async Task<ServiceResult<OrderDetailsDto>> GetOrderDetailsAsync(int id, int? branchId, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetFullOrderDetailsAsync(id, ct);
            if (order is null)
                return _serviceResultHandler.NotFound<OrderDetailsDto>("Order not found.");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                return _serviceResultHandler.Forbidden<OrderDetailsDto>("This order does not belong to your branch.");

            var dto = new OrderDetailsDto
            {
                Id = order.Id,
                InvoiceNumber = order.Invoice?.InvoiceNumber ?? string.Empty,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.CreatedAt,
                CustomerName = order.Customer != null ? $"{order.Customer.FName} {order.Customer.LName}".Trim() : string.Empty,
                CashierName = order.Cashier?.UserName ?? string.Empty,
                BranchName = order.Branch?.Name,
                PaymentMethod = order.PaymentMethod.ToString()
            };

            return _serviceResultHandler.Success(dto);
        }

        public async Task<ServiceResult<PagedResult<OrderDetailsDto>>> GetOrdersPagedAsync(OrdersPaginationRequest query, int? branchId, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Orders.GetOrdersPagedAsync(query.PageNumber, query.PageSize, query.SortBy, query.Descending, branchId, ct);

            var list = items.Select(o => new OrderDetailsDto
            {
                Id = o.Id,
                InvoiceNumber = o.Invoice?.InvoiceNumber ?? string.Empty,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                OrderDate = o.CreatedAt,
                CustomerName = o.Customer != null ? $"{o.Customer.FName} {o.Customer.LName}".Trim() : string.Empty,
                CashierName = o.Cashier?.UserName ?? string.Empty,
                BranchName = o.Branch?.Name,
                PaymentMethod = o.PaymentMethod.ToString()
            }).ToList();

            var result = PagedResult<OrderDetailsDto>.Create(list, totalCount, query.PageNumber, query.PageSize);
            return _serviceResultHandler.Success(result);
        }

        public async Task<ServiceResult<OrdersSummaryDto>> GetOrdersSummaryAsync(int? branchId, CancellationToken ct = default)
        {
            var total = await _unitOfWork.Orders.GetTotalOrdersCountAsync(branchId, ct);
            var completed = await _unitOfWork.Orders.GetCompletedOrdersCountAsync(branchId, ct);
            var pending = await _unitOfWork.Orders.GetPendingOrdersCountAsync(branchId, ct);
            var revenue = await _unitOfWork.Orders.GetTotalRevenueAsync(branchId, ct);

            var dto = new OrdersSummaryDto
            {
                TotalOrders = total,
                Completed = completed,
                Pending = pending,
                TotalRevenue = revenue
            };

            return _serviceResultHandler.Success(dto);
        }

        public async Task<ServiceResult<bool>> UpdateOrderAsync(int id, int? branchId, UpdateOrderDto dto, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id, ct);
            if (order is null)
                return _serviceResultHandler.NotFound<bool>("Order not found.");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                return _serviceResultHandler.Forbidden<bool>("This order does not belong to your branch.");

            order.Status = dto.Status;
            order.PaymentMethod = dto.PaymentMethod;
            order.Note = dto.Note;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            return _serviceResultHandler.Success(true);
        }

        private decimal GetShippingCost() => _configuration.GetValue<decimal>("CartSettings:FixedShippingCost", 10.0m);
        private static string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}