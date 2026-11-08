using Gatherly.Core.DTOs.Expenses;

namespace Gatherly.Core.Services
{
    public interface IExpenseShareService
    {
        Task<ExpenseShareResponseDto> GetShareAsync(
    int eventId,
    int userId,
    CancellationToken cancellationToken = default);
    }
}