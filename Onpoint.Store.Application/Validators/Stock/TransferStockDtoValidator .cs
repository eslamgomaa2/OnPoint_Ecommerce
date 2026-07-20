using FluentValidation;
using Onpoint.Store.Application.DTOs.Stock;

namespace Onpoint.Store.Application.Validators.Stock
{
    public class TransferStockDtoValidator : AbstractValidator<TransferStockDto>
    {
        public TransferStockDtoValidator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.FromBranchId)
                .NotEqual(x => x.ToBranchId)
                .WithMessage("Source and destination branches must be different.");
        }
    }
}