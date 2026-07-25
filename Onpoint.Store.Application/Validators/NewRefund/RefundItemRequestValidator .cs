using FluentValidation;
using Onpoint.Store.Application.DTOs.Refund;

namespace Onpoint.Store.Application.Validators.Refund
{
    public class RefundItemRequestValidator : AbstractValidator<RefundItemRequestDto>
    {
        public RefundItemRequestValidator()
        {
            RuleFor(x => x.OrderItemId)
                .GreaterThan(0).WithMessage("Order item ID must be greater than 0.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}