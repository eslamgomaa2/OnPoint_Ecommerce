using FluentValidation;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.ProductAttribute
{
    public class UpdateProductAttributeDtoValidator : AbstractValidator<UpdateProductAttributeDto>
    {
        public UpdateProductAttributeDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Key)
                .NotEmpty()
                .MaximumLength(120)
                .Matches("^[a-z0-9_-]+$")
                .WithMessage("Key must be lowercase letters, numbers, hyphens, or underscores only.");

            RuleFor(x => x)
                .MustAsync(async (dto, ct) => !await unitOfWork.ProductAttributes.KeyExistsAsync(dto.Key, dto.Id, ct))
                .WithMessage("An attribute with this key already exists.");

            RuleFor(x => x.ValueType).IsInEnum();
        }
    }
}