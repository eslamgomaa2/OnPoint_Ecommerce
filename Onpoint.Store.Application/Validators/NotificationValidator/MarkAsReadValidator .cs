using FluentValidation;
using Onpoint.Store.Application.DTOs.Notification;

namespace Application.Validators;

public class MarkAsReadValidator : AbstractValidator<MarkAsReadDto>
{
    public MarkAsReadValidator()
    {
        RuleFor(x => x.NotificationId)
            .GreaterThan(0).WithMessage("Valid notification ID is required");
    }
}