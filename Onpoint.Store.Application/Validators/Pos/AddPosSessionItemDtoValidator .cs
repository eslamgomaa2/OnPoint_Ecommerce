using FluentValidation;
using Onpoint.Store.Application.DTOs.PosSession;

namespace Onpoint.Store.Application.Validators.PosSession
{
    public class AddPosSessionItemDtoValidator : AbstractValidator<AddPosSessionItemDto>
    {
        public AddPosSessionItemDtoValidator()
        {
            RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("Valid product is required");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1");
        }
    }
}