using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Validators.Product
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(220)
                .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Invalid slug format.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Please select a valid category.");


            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count == 0 || images.Any(i => i.IsPrimary))
                .WithMessage("At least one image must be marked as primary.");


            RuleFor(x => x.Discount)
                .SetValidator(new CreateDiscountDtoValidator()!)
                .When(x => x.Discount != null);
        }
    }
}
