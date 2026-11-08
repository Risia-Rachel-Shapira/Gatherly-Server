using FluentValidation;
using Gatherly.Core.DTOs.Expenses;

namespace Gatherly.Services.Validators
{
    public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
    {
        public CreateExpenseValidator()
        {
            RuleFor(x => x.EventId)
                .GreaterThan(0);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}