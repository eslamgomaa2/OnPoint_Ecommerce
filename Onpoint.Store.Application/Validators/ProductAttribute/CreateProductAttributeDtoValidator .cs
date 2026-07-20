using FluentValidation;
using Onpoint.Store.Application.DTOs.ProductAttribute;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.ProductAttribute
{
    public class CreateProductAttributeDtoValidator : AbstractValidator<CreateProductAttributeDto>
    {
        public CreateProductAttributeDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Key)
                .NotEmpty()
                .MaximumLength(120)
                .Matches("^[a-z0-9_-]+$")
                .WithMessage("Key must be lowercase letters, numbers, hyphens, or underscores only (e.g. 'color', 'memory_size').");

            RuleFor(x => x.Key)
                .MustAsync(async (key, ct) => !await unitOfWork.ProductAttributes.KeyExistsAsync(key, null, ct))
                .WithMessage("An attribute with this key already exists.");

            RuleFor(x => x.ValueType).IsInEnum();
        }
    }
}