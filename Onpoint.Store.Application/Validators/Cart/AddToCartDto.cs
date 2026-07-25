using FluentValidation;
using Onpoint.Store.Application.DTOs.Cart;

namespace Onpoint.Store.Application.Validators.Cart
{
    public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
    {
        public AddToCartDtoValidator()
        {
           

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}
