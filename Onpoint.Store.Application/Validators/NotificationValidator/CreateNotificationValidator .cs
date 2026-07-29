using FluentValidation;
using Onpoint.Store.Application.DTOs.Notification;

namespace Application.Validators;

public class CreateNotificationValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required")
            .MaximumLength(2000).WithMessage("Body must not exceed 2000 characters");
    }
}