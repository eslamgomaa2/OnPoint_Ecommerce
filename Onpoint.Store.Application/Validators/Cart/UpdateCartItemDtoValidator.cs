using FluentValidation;
using Onpoint.Store.Application.DTOs.Cart;

namespace Onpoint.Store.Application.Validators.Cart
{
    public class UpdateCartItemDtoValidator : AbstractValidator<UpdateCartItemDto>
    {
        public UpdateCartItemDtoValidator()
        {


            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative."); // 0 معناها احذفه
        }
    }
}
