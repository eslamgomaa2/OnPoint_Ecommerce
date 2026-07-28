using AutoMapper;
using BuildingBlocks.Results;
using Microsoft.EntityFrameworkCore;
using Onpoint.Store.Application.DTOs.Refund;
using Onpoint.Store.Application.Interfaces;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;
using System.Data;

namespace Onpoint.Store.Application.Services
{
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ServiceResultHandler _serviceResultHandler;

        public RefundService(IUnitOfWork unitOfWork, IMapper mapper, ServiceResultHandler serviceResultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceResultHandler = serviceResultHandler;
        }

        public async Task<ServiceResult<RefundDto>> CreateFullRefundAsync(int userId, int? branchId, int orderId, FullRefundRequestDto dto, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetFullOrderDetailsAsync(orderId);

            if (order == null)
                _serviceResultHandler.BadRequest<RefundDto>($"Order with ID {orderId} not found.");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                _serviceResultHandler.BadRequest<RefundDto>("You can only refund orders from your assigned branch.");

            if (order.Status != OrderStatus.Completed)
                _serviceResultHandler.BadRequest<RefundDto>("Only completed orders can be refunded.");

            if (order.Status == OrderStatus.Refunded)
                _serviceResultHandler.BadRequest<RefundDto>("Order is already fully refunded.");

            var itemsToRefund = order.OrderItems.Where(oi => oi.NetQuantity > 0).ToList();
            if (!itemsToRefund.Any())
                _serviceResultHandler.BadRequest<RefundDto>("No items available to refund.");

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                var refund = new Refund
                {
                    OrderId = orderId,
                    BranchId = order.BranchId,
                    Type = RefundType.Full,
                    Reason = dto.Reason,
                    RefundedAt = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    TotalAmount = itemsToRefund.Sum(oi => oi.NetQuantity * oi.UnitPrice)
                };

                await _unitOfWork.Refunds.AddAsync(refund, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                foreach (var item in itemsToRefund)
                {
                    var refundQty = item.NetQuantity;

                    var refundItem = new RefundItem
                    {
                        RefundId = refund.Id,
                        OrderItemId = item.Id,
                        Quantity = refundQty,
                        UnitPrice = item.UnitPrice
                    };

                    await _unitOfWork.RefundItems.AddAsync(refundItem, ct);

                    item.RefundedQuantity += refundQty;

                    // ⚠️ UPDATED: Get stock from variant, not product
                    var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(
                        item.ProductId,
                        item.ProductVariantId,
                        order.BranchId);

                    if (stock != null)
                        stock.Quantity += refundQty;
                }

                order.Status = OrderStatus.Refunded;

                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = orderId,
                    Status = OrderStatus.Refunded,
                    EventName = EventName.Refunded,
                    Description = $"Full refund processed. Reason: {dto.Reason}",
                    EventTime = DateTime.UtcNow,
                    PerformedByUserId = userId
                });

                await _unitOfWork.SaveChangesAsync(ct);

                await _unitOfWork.CommitTransactionAsync(ct);

                var result = await _unitOfWork.Refunds.GetByIdAsync(refund.Id,
                    include: q => q.Include(r => r.RefundItems)
                                   .ThenInclude(ri => ri.OrderItem),
                    ct: ct);

                return _serviceResultHandler.Success(_mapper.Map<RefundDto>(result!));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<ServiceResult<RefundDto>> CreatePartialRefundAsync(int userId, int? branchId, int orderId, PartialRefundRequestDto dto, CancellationToken ct = default)
        {
            var order = await _unitOfWork.Orders.GetFullOrderDetailsAsync(orderId);

            if (order == null)
                _serviceResultHandler.BadRequest<RefundDto>($"Order with ID {orderId} not found.");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                _serviceResultHandler.BadRequest<RefundDto>("You can only refund orders from your assigned branch.");

            if (order.Status != OrderStatus.Completed)
                _serviceResultHandler.BadRequest<RefundDto>("Only completed orders can be refunded.");

            if (dto.Items == null || !dto.Items.Any())
                _serviceResultHandler.BadRequest<RefundDto>("At least one item must be specified for partial refund.");

            foreach (var itemDto in dto.Items)
            {
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.OrderItemId);
                if (orderItem == null)
                    throw new InvalidOperationException($"Order item with ID {itemDto.OrderItemId} not found in this order.");

                if (itemDto.Quantity > orderItem.NetQuantity)
                    throw new InvalidOperationException(
                        $"Cannot refund {itemDto.Quantity} of {orderItem.ProductName}. Only {orderItem.NetQuantity} available.");
            }

            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                var refund = new Refund
                {
                    OrderId = orderId,
                    BranchId = order.BranchId,
                    Type = RefundType.Partial,
                    Reason = dto.Reason,
                    RefundedAt = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    TotalAmount = 0
                };

                await _unitOfWork.Refunds.AddAsync(refund, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                decimal totalAmount = 0;

                foreach (var itemDto in dto.Items)
                {
                    var orderItem = order.OrderItems.First(oi => oi.Id == itemDto.OrderItemId);

                    var refundItem = new RefundItem
                    {
                        RefundId = refund.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = itemDto.Quantity,
                        UnitPrice = orderItem.UnitPrice
                    };

                    await _unitOfWork.RefundItems.AddAsync(refundItem, ct);

                    orderItem.RefundedQuantity += itemDto.Quantity;
                    totalAmount += itemDto.Quantity * orderItem.UnitPrice;

                    // ⚠️ UPDATED: Get stock from variant, not product
                    var stock = await _unitOfWork.Stocks.GetByProductVariantAndBranchAsync(
                        orderItem.ProductId,
                        orderItem.ProductVariantId,
                        order.BranchId);

                    if (stock != null)
                        stock.Quantity += itemDto.Quantity;
                }

                refund.TotalAmount = totalAmount;

                bool allItemsFullyRefunded = order.OrderItems.All(oi => oi.RefundedQuantity >= oi.Quantity);
                if (allItemsFullyRefunded)
                    order.Status = OrderStatus.Refunded;

                order.StatusHistory.Add(new OrderStatusHistory
                {
                    OrderId = orderId,
                    Status = allItemsFullyRefunded ? OrderStatus.Refunded : order.Status,
                    EventName = EventName.Refunded,
                    Description = $"Partial refund processed. Reason: {dto.Reason}",
                    EventTime = DateTime.UtcNow,
                    PerformedByUserId = userId
                });

                await _unitOfWork.SaveChangesAsync(ct);

                await _unitOfWork.CommitTransactionAsync(ct);

                var result = await _unitOfWork.Refunds.GetByIdAsync(refund.Id,
                    include: q => q.Include(r => r.RefundItems)
                                   .ThenInclude(ri => ri.OrderItem),
                    ct: ct);

                return _serviceResultHandler.Success(_mapper.Map<RefundDto>(result!));
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<ServiceResult<IReadOnlyList<RefundDto>>> GetOrderRefundsAsync(int orderId, CancellationToken ct = default)
        {
            var refunds = await _unitOfWork.Refunds.FindAsync(
                r => r.OrderId == orderId,
                include: q => q.Include(r => r.RefundItems)
                               .ThenInclude(ri => ri.OrderItem),
                orderBy: q => q.OrderByDescending(r => r.RefundedAt),
                ct: ct);

            return _serviceResultHandler.Success(_mapper.Map<IReadOnlyList<RefundDto>>(refunds));
        }
    }
}