using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;

public class CreateDiscountDtoValidator : AbstractValidator<CreateDiscountDto>
{
    public CreateDiscountDtoValidator()
    {
        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Discount percentage must be between 0 and 100.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Start date cannot be in the past.");
    }
}