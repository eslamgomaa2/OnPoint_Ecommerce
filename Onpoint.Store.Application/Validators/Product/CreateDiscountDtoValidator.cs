using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Validators.Product
{
    public class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
    {
        public CreateDiscountDtoValidator()
        {
            RuleFor(x => x.DiscountPercentage)
                .InclusiveBetween(1, 99).WithMessage("Discount must be between 1% and 99%.");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date.");
        }
    }
}
