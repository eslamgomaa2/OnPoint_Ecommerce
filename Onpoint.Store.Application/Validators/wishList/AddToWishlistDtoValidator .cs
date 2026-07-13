using FluentValidation;
using Onpoint.Store.Application.DTOs.Wishlist;

namespace Onpoint.Store.Application.Validators.Wishlist
{
    public class AddToWishlistDtoValidator : AbstractValidator<AddToWishlistDto>
    {
        public AddToWishlistDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Valid Product ID is required.");
        }
    }
}