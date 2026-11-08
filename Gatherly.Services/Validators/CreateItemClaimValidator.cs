using FluentValidation;
using Gatherly.Core.DTOs.ItemClaims;

namespace Gatherly.Services.Validators
{
    public class CreateItemClaimValidator : AbstractValidator<CreateItemClaimDto>
    {
        public CreateItemClaimValidator()
        {
            RuleFor(x => x.ItemId)
                .GreaterThan(0);

            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }
}