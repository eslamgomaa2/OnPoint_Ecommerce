using FluentValidation;
using Onpoint.Store.Application.DTOs.Discount;

namespace Onpoint.Store.Application.Validators.Discount
{
    public class UpdateDiscountDtoValidator : AbstractValidator<UpdateDiscountDto>
    {
        public UpdateDiscountDtoValidator()
        {

            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.StartDate)
                .NotEmpty();

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate.");
        }
    }
}