using FluentValidation;
using Onpoint.Store.Application.DTOs.Address;

namespace Onpoint.Store.Application.Validators.Address
{
    public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
    {
        public CreateAddressDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(50).WithMessage("Title must not exceed 50 characters.");

            RuleFor(x => x.Governorate)
                .NotEmpty().WithMessage("Governorate is required.");

            RuleFor(x => x.Area)
                .NotEmpty().WithMessage("Area is required.");

            RuleFor(x => x.Block)
                .NotEmpty().WithMessage("Block is required.");

            RuleFor(x => x.HouseNumber)
                .NotEmpty().WithMessage("House Number is required.");
        }
    }
}