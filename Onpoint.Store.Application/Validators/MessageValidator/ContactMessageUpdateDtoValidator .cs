using FluentValidation;
using Onpoint.Store.Application.DTOs.Contactmessage;

namespace Onpoint.Store.Application.Validators.MessageValidator
{

    public class ContactMessageUpdateDtoValidator : AbstractValidator<ContactMessageUpdateDto>
    {
        public ContactMessageUpdateDtoValidator()
        {


            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(200);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(30)
                .Matches(@"^[0-9+\-\s()]*$").WithMessage("Phone number format is invalid.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message cannot be empty.")
                .MaximumLength(2000);
        }
    }

}
