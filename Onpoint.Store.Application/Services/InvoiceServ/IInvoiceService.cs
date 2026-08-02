
using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Invoice;

namespace Onpoint.Store.Application.Services.InvoiceServ
{

    public interface IInvoiceService
    {
        Task<ServiceResult<PagedResult<InvoiceListItemDto>>> GetInvoicesPagedAsync(int? Branchid, InvoiceFilter filter, CancellationToken ct = default);

        Task<ServiceResult<InvoiceDetailsDto>> GetInvoiceDetailsAsync(
            int id,
            int? branchId = null,
            CancellationToken ct = default);
        public Task<ServiceResult<InvoiceStatisticsDto>> GetInvoiceStatisticsAsync(int? branchId, CancellationToken ct = default);


    }
}