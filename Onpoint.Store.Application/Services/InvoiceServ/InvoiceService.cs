
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Invoice;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.InvoiceServ
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ServiceResultHandler _serviceResultHandler;

        public InvoiceService(IUnitOfWork unitOfWork, ServiceResultHandler serviceResultHandler)
        {
            _unitOfWork = unitOfWork;
            _serviceResultHandler = serviceResultHandler;
        }

        public async Task<ServiceResult<PagedResult<InvoiceListItemDto>>> GetInvoicesPagedAsync(
            int? branchId,
            InvoiceFilter filter,
            CancellationToken ct = default)
        {
            var (items, totalCount) = await _unitOfWork.Invoices.GetInvoicesPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                filter.SearchTerm,
                filter.DateFrom,
                filter.DateTo,
               branchId,
                filter.OrderSource,
                ct);

            var list = items.Select(o => new InvoiceListItemDto
            {
                Id = o.Id,
                InvoiceNumber = o.InvoiceNumber,
                IssueDate = o.CreatedAt,
                TotalAmount = o.TotalAmount,
                OrderId = o.Id,
                CustomerName = o.Customer != null
                    ? $"{o.Customer.FName} {o.Customer.LName}".Trim()
                    : null,
                BranchName = o.Branch?.Name,
                Status = o.Status.ToString()

            }).ToList();

            var result = PagedResult<InvoiceListItemDto>.Create(list, totalCount, filter.PageNumber, filter.PageSize);
            return _serviceResultHandler.Success(result);
        }

        public async Task<ServiceResult<InvoiceDetailsDto>> GetInvoiceDetailsAsync(
            int id,
            int? branchId = null,
            CancellationToken ct = default)
        {
            var order = await _unitOfWork.Invoices.GetInvoiceDetailsAsync(id, ct);

            if (order is null)
                return _serviceResultHandler.NotFound<InvoiceDetailsDto>("Invoice not found.");

            if (branchId.HasValue && order.BranchId != branchId.Value)
                return _serviceResultHandler.Forbidden<InvoiceDetailsDto>("This invoice does not belong to your branch.");

            var dto = new InvoiceDetailsDto
            {
                Id = order.Id,
                InvoiceNumber = order.InvoiceNumber,
                IssueDate = order.CreatedAt,
                OrderId = order.Id,
                CustomerName = order.Customer != null
                    ? $"{order.Customer.FName} {order.Customer.LName}".Trim()
                    : null,
                CashierName = order.Cashier?.UserName,
                BranchName = order.Branch?.Name,
                PaymentMethod = order.PaymentMethod.ToString(),
                Status = order.Status.ToString(),
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                ShippingCost = order.ShippingCost,
                TaxAmount = order.TaxAmount,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new InvoiceItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? oi.ProductName ?? "Unknown",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            return _serviceResultHandler.Success(dto);
        }
    }
}