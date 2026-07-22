using FluentValidation;
using Onpoint.Store.Application.DTOs.Product;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Product
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CategoryId).GreaterThan(0);



        }
    }
}