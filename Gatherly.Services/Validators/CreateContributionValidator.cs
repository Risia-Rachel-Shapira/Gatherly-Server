using FluentValidation;
using Gatherly.Core.DTOs.Contributions;

namespace Gatherly.Services.Validators
{
    public class CreateContributionValidator : AbstractValidator<CreateContributionDto>
    {
        public CreateContributionValidator()
        {
            RuleFor(x => x.EventId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0);
        }
    }
}