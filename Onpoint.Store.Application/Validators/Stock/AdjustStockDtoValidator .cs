using FluentValidation;
using Onpoint.Store.Application.DTOs.Stock;

namespace Onpoint.Store.Application.Validators.Stock
{
    public class AdjustStockDtoValidator : AbstractValidator<AdjustStockDto>
    {
        public AdjustStockDtoValidator()
        {
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.AdjustmentType).IsInEnum();
        }
    }
}