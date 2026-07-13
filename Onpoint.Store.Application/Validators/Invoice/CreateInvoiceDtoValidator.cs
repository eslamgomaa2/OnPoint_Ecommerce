using FluentValidation;
using Onpoint.Store.Application.DTOs.Invoice;

namespace Onpoint.Store.Application.Validators.Invoice
{
    public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
    {
        public CreateInvoiceDtoValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0).WithMessage("OrderId is required");

            RuleFor(x => x.CustomerTaxNumber)
                .NotEmpty().When(x => x.IsTaxable)
                .WithMessage("Tax Number is required for taxable invoices");
        }
    }
}