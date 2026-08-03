using FluentValidation;
using Onpoint.Store.Application.DTOs.StoreSettings;

namespace Onpoint.Store.Application.Validators.StoreSettings
{
    public class StoreSettingsValidator : AbstractValidator<StoreSettingsUpdateDto>
    {
        public StoreSettingsValidator()
        {
            RuleFor(x => x.Settings)
                .NotEmpty().WithMessage("Settings collection cannot be empty.");


            RuleForEach(x => x.Settings).ChildRules(setting =>
            {
                setting.RuleFor(s => s.Key)
                    .NotEmpty().WithMessage("Setting key is required.")
                    .MaximumLength(100).WithMessage("Setting key cannot exceed 100 characters.");

                setting.RuleFor(s => s.Value)
                    .NotNull().WithMessage("Setting value cannot be null.");
            });
        }
    }
}