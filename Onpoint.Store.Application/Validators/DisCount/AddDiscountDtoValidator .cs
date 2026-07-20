using FluentValidation;
using Onpoint.Store.Application.DTOs.Discount;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Discount
{
    public class AddDiscountDtoValidator : AbstractValidator<AddDiscountDto>
    {
        public AddDiscountDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.ProductId).GreaterThan(0);

            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be after start date.");

            RuleFor(x => x)
                .MustAsync(async (dto, ct) =>
                {
                    var product = await unitOfWork.Products.GetByIdAsync(dto.ProductId, ct);
                    return product != null;
                })
                .WithMessage("Product not found.");

            RuleFor(x => x)
                .MustAsync(async (dto, ct) =>
                {
                    var hasOverlap = await unitOfWork.Discounts.HasOverlappingDiscountAsync(
                        dto.ProductId, dto.StartDate, dto.EndDate, ct);

                    return !hasOverlap;
                })
                .WithMessage("This discount period overlaps with an existing active discount for this product.");
        }
    }
}