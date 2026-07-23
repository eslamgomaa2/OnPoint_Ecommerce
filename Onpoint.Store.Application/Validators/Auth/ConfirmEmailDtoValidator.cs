using FluentValidation;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Validators.Auth
{
    public class ConfirmEmailDtoValidator : AbstractValidator<ConfirmEmailDto>
    {
        public ConfirmEmailDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Verification token is required.");
        }
    }

}
