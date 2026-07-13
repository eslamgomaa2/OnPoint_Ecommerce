using FluentValidation;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Validators.Auth
{
    public class ConfirmEmailDtoValidator : AbstractValidator<ConfirmEmailDto>
    {
        public ConfirmEmailDtoValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Invalid User ID.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Verification token is required.");
        }
    }

}
