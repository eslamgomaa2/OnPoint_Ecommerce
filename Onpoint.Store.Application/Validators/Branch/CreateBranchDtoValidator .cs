using FluentValidation;
using Onpoint.Store.Application.DTOs.Branch;

namespace Onpoint.Store.Application.Validators.Branch
{
    public class CreateBranchDtoValidator : AbstractValidator<CreateBranchDto>
    {
        public CreateBranchDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Branch name is required.")
                .MaximumLength(200).WithMessage("Branch name must not exceed 200 characters.");

            RuleFor(x => x.Address)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Address))
                .WithMessage("Address must not exceed 500 characters.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Phone must not exceed 20 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format.")
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Email must not exceed 100 characters.");

            RuleFor(x => x.GoogleMapLocation)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.GoogleMapLocation))
                .WithMessage("Google Map location must not exceed 500 characters.");

            RuleFor(x => x.WorkingHours)
                .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.WorkingHours))
                .WithMessage("Working hours must not exceed 100 characters.");


        }
    }
}
