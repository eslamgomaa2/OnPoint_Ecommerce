using FluentValidation;
using Onpoint.Store.Application.DTOs.PosSession;

namespace Onpoint.Store.Application.Validators.Pos
{
    public class CompletePosSessionDtoValidator : AbstractValidator<CompletePosSessionDto>
    {
        private const int CashPaymentMethodId = 0;

        public CompletePosSessionDtoValidator()
        {
            RuleFor(x => x.PaymentMethodId)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Payment method is required");

            RuleFor(x => x.AmountReceived)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Amount received cannot be negative");

            RuleFor(x => x.AmountReceived)
                .GreaterThan(0)
                .When(x => x.PaymentMethodId == CashPaymentMethodId)
                .WithMessage("Amount received is required for cash payments");
        }
    }
}