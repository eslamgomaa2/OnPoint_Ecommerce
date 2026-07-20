using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Enums;

namespace Onpoint.Store.Application.Validators.Product
{
    public class UpdateVariantDtoValidator : AbstractValidator<UpdateProductVariantDto>
    {
        public UpdateVariantDtoValidator()
        {
            RuleFor(x => x.Price).GreaterThan(0);

            RuleFor(x => x.Barcode)
                .NotEmpty()
                .When(x => x.BarcodeMode == CodeGenerationMode.Manual)
                .WithMessage("Barcode is required when BarcodeMode is Manual.");

            RuleFor(x => x.Attributes)
                .NotEmpty()
                .WithMessage("A variant must have at least one attribute (e.g. Color, Size).");
        }
    }
}