using FluentValidation;
using Onpoint.Store.Application.DTOs.Refund;

namespace Onpoint.Store.Application.Validators.Refund
{
    public class PartialRefundRequestValidator : AbstractValidator<PartialRefundRequestDto>
    {
        public PartialRefundRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required for partial refund.");

            RuleForEach(x => x.Items).SetValidator(new RefundItemRequestValidator());
        }
    }
}