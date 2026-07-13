using FluentValidation;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Validators.Auth
{
    public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email Is Requird")
                .EmailAddress().WithMessage("The email format is incorrect.")
                .MaximumLength(256).WithMessage("Email is Too Long");
        }
    }
}
