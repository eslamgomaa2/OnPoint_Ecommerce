using FluentValidation;
using Onpoint.Store.Application.DTOs.PaymentTransaction;

namespace Onpoint.Store.Application.Validators.PaymentTransaction
{
    public class UpdatePaymentStatusDtoValidator : AbstractValidator<UpdatePaymentStatusDto>
    {
        public UpdatePaymentStatusDtoValidator()
        {
            RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required");
        }
    }
}