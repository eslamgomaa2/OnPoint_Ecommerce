using FluentValidation;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Validators.Auth
{
    public class VerifyResetCodeDtoValidator : AbstractValidator<VerifyResetCodeDto>
    {
        public VerifyResetCodeDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Verification code is required.")
                .Length(4, 6).WithMessage("Verification code must be between 4 and 6 characters.")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("Verification code contains invalid characters.");
        }
    }
}
