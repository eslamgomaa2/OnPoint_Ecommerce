using BuildingBlocks.Results;
using Onpoint.Store.Application.DTOs.Invoice;

namespace Onpoint.Store.Application.Services.InvoiceServ
{
    public interface IInvoiceService
    {
        Task<ServiceResult<InvoiceDto>> GenerateInvoiceAsync(CreateInvoiceDto dto);
        Task<ServiceResult<InvoiceDto>> GetByOrderIdAsync(int orderId);
        Task<ServiceResult<InvoiceDto>> GetByInvoiceNumberAsync(string invoiceNumber);
    }
}