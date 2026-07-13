using FluentValidation;
using Onpoint.Store.Application.DTOs.Order;

namespace Onpoint.Store.Application.Validators.Order
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.AddressId).GreaterThan(0).WithMessage("Address ID is required");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required");
            RuleFor(x => x.PaymentMethod).IsInEnum().WithMessage("Invalid payment method");
        }
    }
}
