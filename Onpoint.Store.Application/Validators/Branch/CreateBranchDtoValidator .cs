using FluentValidation;
using Onpoint.Store.Application.DTOs.Branch;

namespace Onpoint.Store.Application.Validators.Branch
{
    public class CreateBranchDtoValidator : AbstractValidator<CreateBranchDto>
    {
        public CreateBranchDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("Branch name is required");
            RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
}
