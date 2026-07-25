using FluentValidation;
using Onpoint.Store.Application.DTOs.Refund;

namespace Onpoint.Store.Application.Validators
{
    public class PartialRefundRequestValidator : AbstractValidator<PartialRefundRequestDto>
    {
        public PartialRefundRequestValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required for partial refund.");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.OrderItemId).GreaterThan(0);
                item.RuleFor(x => x.Quantity).GreaterThan(0);
            });
        }
    }
}