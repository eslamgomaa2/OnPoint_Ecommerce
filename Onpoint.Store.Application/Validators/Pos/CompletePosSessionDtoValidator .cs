using FluentValidation;
using Onpoint.Store.Application.DTOs.PosSession;

namespace Onpoint.Store.Application.Validators.Pos
{
    public class CompletePosSessionDtoValidator : AbstractValidator<CompletePosSessionDto>
    {
        public CompletePosSessionDtoValidator()
        {
            RuleForEach(x => x.Payments).ChildRules(payment =>
            {
                payment.RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than 0");
            });

            RuleFor(x => x.AmountReceived)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Payments.Any(p => p.Method == Domin.Enums.PaymentMethod.Cash))
                .WithMessage("Amount received is required for cash payments");
        }
    }
}
