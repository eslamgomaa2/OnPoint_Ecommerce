using FluentValidation;
using Onpoint.Store.Application.DTOs.StaticPages;

public class UpdateStaticPageDtoValidator : AbstractValidator<UpdateStaticPageDto>
{
    public UpdateStaticPageDtoValidator()
    {


        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");
    }
}