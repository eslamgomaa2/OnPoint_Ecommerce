using FluentValidation;
using Onpoint.Store.Application.DTOs.Stock;
using Onpoint.Store.Domin.Repositories;

namespace Onpoint.Store.Application.Validators.Stock
{
    public class InitializeStockDtoValidator : AbstractValidator<InitializeStockDto>
    {
        public InitializeStockDtoValidator(IUnitOfWork unitOfWork)
        {
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);



            RuleFor(x => x)
                .MustAsync(async (dto, ct) =>
                {
                    var branch = await unitOfWork.Branches.GetByIdAsync(dto.BranchId, ct);
                    return branch != null && branch.IsActive;
                })
                .WithMessage("Branch not found or inactive.");

        }
    }
}