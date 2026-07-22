using FluentValidation;
using Onpoint.Store.Application.DTOs.Brand;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Brand
{
    public class CreateBrandDtoValidator : AbstractValidator<CreateBrandDto>
    {
        public CreateBrandDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Brand name is required.")
                .MaximumLength(100).WithMessage("Brand name must not exceed 100 characters.")
                .MustAsync(async (name, ct) => !await unitOfWork.Brands.NameExistsAsync(name, null, ct))
                    .WithMessage("Brand name already exists.");

            RuleFor(x => x.LogoUrl)
                .MaximumLength(500).WithMessage("Logo URL is too long.")
                .When(x => !string.IsNullOrEmpty(x.LogoUrl));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order cannot be negative.");
        }
    }
}