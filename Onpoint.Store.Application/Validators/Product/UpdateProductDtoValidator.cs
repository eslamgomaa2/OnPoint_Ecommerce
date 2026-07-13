using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;

namespace Onpoint.Store.Application.Validators.Product
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Slug).NotEmpty().MaximumLength(220).Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).GreaterThan(0);
            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count == 0 || images.Any(i => i.IsPrimary))
                .WithMessage("At least one image must be marked as primary.");
            RuleFor(x => x.Discount).SetValidator(new CreateDiscountDtoValidator()!).When(x => x.Discount != null);
        }
    }
}
