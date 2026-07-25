using FluentValidation;
using Onpoint.Store.Application.DTOs.Refund;

namespace Onpoint.Store.Application.Validators.Refund
{
    public class FullRefundRequestValidator : AbstractValidator<FullRefundRequestDto>
    {
        public FullRefundRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters.");
        }
    }
}