using FluentValidation;
using Gatherly.Core.DTOs.Items;

namespace Gatherly.Services.Validators
{
    public class CreateItemValidator : AbstractValidator<CreateItemDto>
    {
        public CreateItemValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.EventId)
                .GreaterThan(0);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);

            RuleFor(x => x.EstimatedCost)
                .GreaterThanOrEqualTo(0)
                .When(x => x.EstimatedCost.HasValue);
        }
    }
}