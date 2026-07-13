using FluentValidation;
using Onpoint.Store.Application.DTOs.Cart;

namespace Onpoint.Store.Application.Validators.Cart
{
    public class ApplyCouponDtoValidator : AbstractValidator<ApplyCouponDto>
    {
        public ApplyCouponDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Coupon code is required.")
                .MaximumLength(50).WithMessage("Coupon code is too long.");
        }
    }
}