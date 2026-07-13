using FluentValidation;
using Onpoint.Store.Application.DTOs.Coupon;

namespace Onpoint.Store.Application.Validators.Coupon
{
    public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
    {
        public CreateCouponDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("Coupon code is required");
            RuleFor(x => x.Value).GreaterThan(0).WithMessage("Discount value must be greater than zero");
            RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0).WithMessage("Minimum order amount must be zero or greater");
            RuleFor(x => x.MaxUses).GreaterThan(0).WithMessage("Max uses must be greater than zero");
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after the start date");
        }
    }
}
