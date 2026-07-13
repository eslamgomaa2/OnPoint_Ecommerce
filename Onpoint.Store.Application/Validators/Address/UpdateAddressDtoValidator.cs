using FluentValidation;
using Onpoint.Store.Application.DTOs.Address;

namespace Onpoint.Store.Application.Validators.Address
{
    public class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressDto>
    {
        public UpdateAddressDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Governorate).NotEmpty();
            RuleFor(x => x.Area).NotEmpty();
            RuleFor(x => x.Block).NotEmpty();
            RuleFor(x => x.HouseNumber).NotEmpty();
        }
    }
}
