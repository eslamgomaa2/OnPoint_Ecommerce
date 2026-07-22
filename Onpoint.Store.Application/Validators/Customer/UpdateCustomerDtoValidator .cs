// Onpoint.Store.Application/Validators/Customer/UpdateCustomerDtoValidator.cs
using FluentValidation;
using Onpoint.Store.Application.DTOs.Customer;

namespace Onpoint.Store.Application.Validators.Customer
{
    public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerDtoValidator()
        {
            RuleFor(x => x.FName)
                .NotEmpty().WithMessage("Customer FirstName is required")
                .MaximumLength(200);
            RuleFor(x => x.LName)
                .NotEmpty().WithMessage("Customer LastName is required")
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .MaximumLength(200);

            RuleFor(x => x.Phone)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Address)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}