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

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateOrderDto> createValidator, ServiceResultHandler serviceResultHandler, IConfiguration configuration)
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

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return _serviceResultHandler.BadRequest<OrderDto>("Cart is empty");
                }

                var defaultBranch = await _unitOfWork.Branches.FirstOrDefaultAsync(b => b.IsDefault && b.IsActive);
                if (defaultBranch == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return _serviceResultHandler.BadRequest<OrderDto>("Default online branch is not configured");
                }

                var stockKeys = cart.Items.Select(i => (i.ProductId, i.ProductVariantId)).Distinct().ToList();
                var stocks = await _unitOfWork.Stocks.GetByProductVariantsAndBranchAsync(stockKeys, defaultBranch.Id);

                foreach (var item in cart.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.ProductId == item.ProductId && s.ProductVariantId == item.ProductVariantId);
                    if (stock == null || stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return _serviceResultHandler.BadRequest<OrderDto>($"Not enough stock for {item.Product?.Name ?? "product"}");
                    }
                }

                decimal discountAmount = 0;
                Coupon? coupon = null;

                if (!string.IsNullOrWhiteSpace(cart.AppliedCouponCode))
                {
                    coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == cart.AppliedCouponCode);

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
                    CustomerId = userId,
                    AddressId = dto.AddressId,
                    PhoneNumber = dto.PhoneNumber,
                    PaymentMethod = dto.PaymentMethod,
                    OrderNumber = GenerateOrderNumber(),
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
                        return _serviceResultHandler.BadRequest<OrderDto>($"Minimum order amount for this coupon is {coupon.MinOrderAmount}");
                    }

                    discountAmount = coupon.DiscountType == CouponType.Percentage ? (order.SubTotal * coupon.Value) / 100 : coupon.Value;
                    if (discountAmount > order.SubTotal) discountAmount = order.SubTotal;
                    order.DiscountAmount = discountAmount;
                }

                order.TotalAmount = order.SubTotal + order.ShippingCost - order.DiscountAmount;

                await _unitOfWork.Orders.AddAsync(order);

                if (dto.PaymentMethod == PaymentMethod.Cash)
                {
                    await FinalizeOrderAsync(order, cart, coupon, default);
                    order.Status = OrderStatus.Pending;
                }

                await _unitOfWork.SaveChangesAsync();
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

        public async Task<ServiceResult<bool>> ProcessPaymentSuccessAsync(int orderId, string transactionId)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
                if (order == null) return _serviceResultHandler.NotFound<bool>("Order not found");
                if (order.Status != OrderStatus.Pending) return _serviceResultHandler.BadRequest<bool>("Order is not pending");

                if (order.Transactions.Any(t => t.Status == PaymentStatus.Success))
                    return _serviceResultHandler.Success<bool>(true);

                Coupon? coupon = null;
                if (!string.IsNullOrEmpty(order.CouponCode))
                    coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);

                var cart = await _unitOfWork.Carts.GetUserCartWithItemsAsync(order.CustomerId);

                await FinalizeOrderAsync(order, cart, coupon, default);

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

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _serviceResultHandler.Success<bool>(true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ServiceResult<bool>> CancelOrderAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
                if (order == null) return _serviceResultHandler.NotFound<bool>("Order not found");

                if (order.Status == OrderStatus.Completed)
                    return _serviceResultHandler.BadRequest<bool>("Cannot cancel delivered order");

                if (order.Status == OrderStatus.Pending)
                {
                    var stockKeys = order.OrderItems.Select(i => (i.ProductId, i.ProductVariantId)).Distinct().ToList();
                    var stocks = await _unitOfWork.Stocks.GetByProductVariantsAndBranchAsync(stockKeys, order.BranchId!.Value);

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
                        var coupon = await _unitOfWork.Coupons.FirstOrDefaultAsync(c => c.Code == order.CouponCode);
                        if (coupon != null && coupon.UsedCount > 0)
                        {
                            coupon.UsedCount--;
                            _unitOfWork.Coupons.Update(coupon);
                        }
                    }
                }

                order.Status = OrderStatus.Refunded;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _serviceResultHandler.Success<bool>(true);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        public async Task<ServiceResult<bool>> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            if (newStatus == OrderStatus.Refunded)
            {
                return await CancelOrderAsync(orderId);
            }

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
            if (order == null) return _serviceResultHandler.NotFound<bool>("Order not found");

            var validTransition = (order.Status, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Pending) => true,
                (OrderStatus.Pending, OrderStatus.Completed) => true,
                _ => false
            };

            if (!validTransition)
                return _serviceResultHandler.BadRequest<bool>($"Invalid status transition from {order.Status} to {newStatus}");

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return _serviceResultHandler.Success<bool>(true);
        }

        public async Task<ServiceResult<IEnumerable<OrderDto>>> GetUserOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetUserOrders(userId);
            if (orders == null || !orders.Any()) return _serviceResultHandler.NotFound<IEnumerable<OrderDto>>("No orders found for this user");

            var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);
            return _serviceResultHandler.Success<IEnumerable<OrderDto>>(ordersDto);
        }

        public async Task<ServiceResult<OrderDto>> GetOrderAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId);
            if (order == null) return _serviceResultHandler.NotFound<OrderDto>("Order not found");

            return _serviceResultHandler.Success(_mapper.Map<OrderDto>(order));
        }

        private decimal GetShippingCost() => _configuration.GetValue<decimal>("CartSettings:FixedShippingCost", 10.0m);
        private string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

        public async Task FinalizeOrderAsync(Order order, Cart? cart, Coupon? coupon, CancellationToken ct = default)
        {
            var branchId = order.BranchId ?? await _unitOfWork.Branches.GetDefaultBranchIdAsync(ct);

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
        public async Task<ServiceResult<PagedResult<OrderListItemDto>>> GetDashBoardPagedAsync(GetOrdersQueryDto query, int? branchId, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Orders.GetOrdersPagedAsync(
                query.PageNumber, query.PageSize, query.SortBy, query.Descending, branchId, ct);

            var dto = new PagedResult<OrderListItemDto>
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Items = items.Select(o => new OrderListItemDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.CreatedAt,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                }).ToList()
            };

            return _serviceResultHandler.Success(dto);
        }

        public async Task<ServiceResult<DashBoardSummaryDto>> GetDasheBoardSummaryAsync(int? branchId, CancellationToken ct = default)
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
            var order = await _unitOfWork.Orders.GetOrderDetailsAsync(id, ct);
            if (order is null)
                return _serviceResultHandler.NotFound<OrderDetailsDto>("Order not found");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                return _serviceResultHandler.Forbidden<OrderDetailsDto>("This order does not belong to your branch");

            var dto = new OrderDetailsDto
            {
                Id = order.Id,
                InvoiceNumber = order.Invoice?.InvoiceNumber ?? order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.CreatedAt,
                CustomerName = $"{order.Customer?.FName} {order.Customer?.LName}" ?? string.Empty,
                CashierName = order.Cashier?.UserName ?? string.Empty,
                BranchName = order.Branch?.Name,
                PaymentMethod = order.PaymentMethod.ToString()
            };

            return _serviceResultHandler.Success(dto);
        }





        public async Task<ServiceResult<PagedResult<OrderDetailsDto>>> GetOrdersPagedAsync(OrdersPaginationRequest query, int? branchId, CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Orders.GetOrdersPagedAsync(query.PageNumber, query.PageSize, query.SortBy, query.Descending, branchId, ct);

            var dto = new PagedResult<OrderDetailsDto>
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                Items = items.Select(o => new OrderDetailsDto
                {
                    Id = o.Id,
                    InvoiceNumber = o.Invoice?.InvoiceNumber ?? o.OrderNumber,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.CreatedAt,
                    CustomerName = $"{o.Customer?.FName} {o.Customer?.LName}" ?? string.Empty,
                    CashierName = o.Cashier?.UserName ?? string.Empty,
                    BranchName = o.Branch?.Name,
                    PaymentMethod = o.PaymentMethod.ToString()
                }).ToList()
            };

            return _serviceResultHandler.Success(dto);
        }

        public async Task<ServiceResult<OrdersSummaryDto>> GetOrdersSummaryAsync(
            int? branchId, CancellationToken ct = default)
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



        public async Task<ServiceResult<bool>> UpdateOrderAsync(
            int id, int? branchId, UpdateOrderDto dto, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id, ct);
            if (order is null)
                return _serviceResultHandler.NotFound<bool>("Order not found");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                return _serviceResultHandler.Forbidden<bool>("This order does not belong to your branch");

            order.Status = dto.Status;
            order.PaymentMethod = dto.PaymentMethod;
            order.Note = dto.Note;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(ct);

            return _serviceResultHandler.Success(true);
        }
    }

}

