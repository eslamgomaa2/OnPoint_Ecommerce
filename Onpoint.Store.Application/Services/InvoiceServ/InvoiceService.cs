using AutoMapper;
using BuildingBlocks.Results;
using FluentValidation;
using Onpoint.Store.Application.DTOs.Invoice;
using Onpoint.Store.Domin.Entities;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Services.InvoiceServ
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateInvoiceDto> _validator;
        private readonly ServiceResultHandler _serviceResultHandler;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<CreateInvoiceDto> validator, ServiceResultHandler serviceResultHandler)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
            _serviceResultHandler = serviceResultHandler;
        }

        public async Task<ServiceResult<InvoiceDto>> GenerateInvoiceAsync(CreateInvoiceDto dto)
        {
            await _validator.ValidateAndThrowAsync(dto);

            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
                return _serviceResultHandler.NotFound<InvoiceDto>("Order not found");

            var existingInvoice = await _unitOfWork.Invoices.GetByOrderIdAsync(dto.OrderId);
            if (existingInvoice != null)
                return _serviceResultHandler.BadRequest<InvoiceDto>("Invoice already generated for this order");

            var invoice = new Invoice
            {
                OrderId = dto.OrderId,
                InvoiceNumber = GenerateInvoiceNumber(),
                SubTotal = order.SubTotal,
                IsTaxable = dto.IsTaxable,
                TaxPercentage = dto.IsTaxable ? 15m : 0m,
                CustomerTaxNumber = dto.CustomerTaxNumber,
                CustomerCompanyName = dto.CustomerCompanyName,
                IssuedAt = DateTime.UtcNow
            };

            invoice.TaxAmount = invoice.IsTaxable ? (invoice.SubTotal * invoice.TaxPercentage) / 100 : 0;
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return _serviceResultHandler.Created(_mapper.Map<InvoiceDto>(invoice));
        }

        public async Task<ServiceResult<InvoiceDto>> GetByOrderIdAsync(int orderId)
        {
            var invoice = await _unitOfWork.Invoices.GetByOrderIdAsync(orderId);
            if (invoice == null)
                return _serviceResultHandler.NotFound<InvoiceDto>("Invoice not found for this order");

            return _serviceResultHandler.Success(_mapper.Map<InvoiceDto>(invoice));
        }

        public async Task<ServiceResult<InvoiceDto>> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            var invoice = await _unitOfWork.Invoices.GetByInvoiceNumberAsync(invoiceNumber);
            if (invoice == null)
                return _serviceResultHandler.NotFound<InvoiceDto>("Invoice not found");

            return _serviceResultHandler.Success(_mapper.Map<InvoiceDto>(invoice));
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(10, 99)}";
        }
    }
}