using FluentValidation;
using Onpoint.Store.Application.DTOs.Product.BranchManger;
using Onpoint.Store.Domin.Enums;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product
{
    public class UpdateProductByBranchManagerDtoValidator : AbstractValidator<UpdateProductByBranchManagerDto>
    {
        public UpdateProductByBranchManagerDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).GreaterThan(0);

            RuleFor(x => x.Images)
                .Must(images => images == null || images.Count == 0 || images.Any(i => i.IsPrimary))
                .WithMessage("At least one image must be marked as primary.");

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .When(x => x.BarcodeMode == CodeGenerationMode.Manual)
                .WithMessage("Barcode is required when BarcodeMode is Manual.");

            RuleFor(x => x.Discount)
                .SetValidator(new CreateDiscountDtoValidator()!)
                .When(x => x.Discount != null);

            RuleForEach(x => x.Variants)
                .ChildRules(v =>
                {
                    v.RuleFor(x => x.Price).GreaterThan(0);
                    v.RuleFor(x => x.Barcode)
                        .NotEmpty()
                        .When(x => x.BarcodeMode == CodeGenerationMode.Manual)
                        .WithMessage("Variant barcode is required when BarcodeMode is Manual.");
                });
        }
    }
}