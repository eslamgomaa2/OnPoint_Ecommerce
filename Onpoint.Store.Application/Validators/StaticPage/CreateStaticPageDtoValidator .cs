using FluentValidation;
using Onpoint.Store.Application.DTOs.StaticPages;

namespace Onpoint.Store.Application.Validators.StaticPage
{
    public class CreateStaticPageDtoValidator : AbstractValidator<CreateStaticPageDto>
    {
        public CreateStaticPageDtoValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid page type.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.");
        }
    }


}