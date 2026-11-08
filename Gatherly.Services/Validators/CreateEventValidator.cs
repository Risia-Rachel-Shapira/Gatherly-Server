using FluentValidation;
using Gatherly.Core.DTOs.Events;

namespace Gatherly.Services.Validators
{
    public class CreateEventValidator : AbstractValidator<CreateEventDto>
    {
        public CreateEventValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Date)
                .NotEmpty();

            RuleFor(x => x.Budget)
                .GreaterThan(0)
                .When(x => x.Budget.HasValue);
        }
    }
}