using FluentValidation;
using Onpoint.Store.Application.DTOs.Auth;

namespace Onpoint.Store.Application.Validators.Auth
{
    public class ExternalLoginDtoValidator : AbstractValidator<ExternalLoginDto>
    {
        public ExternalLoginDtoValidator()
        {
            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required.")
                .Must(BeAValidProvider).WithMessage("Invalid provider. Must be Google, Facebook, or Apple.");

            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("External token is required.");
        }

        private bool BeAValidProvider(string provider)
        {
            return provider.Equals("Google", StringComparison.OrdinalIgnoreCase) ||
                   provider.Equals("Facebook", StringComparison.OrdinalIgnoreCase) ||
                   provider.Equals("Apple", StringComparison.OrdinalIgnoreCase);
        }
    }
}
