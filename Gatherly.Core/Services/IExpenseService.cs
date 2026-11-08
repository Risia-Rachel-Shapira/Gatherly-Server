using Gatherly.Core.DTOs.Expenses;

namespace Gatherly.Core.Services
{
    public interface IExpenseService
    {
        Task<ExpenseResponseDto> CreateAsync(
            CreateExpenseDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<List<ExpenseResponseDto>> GetByEventAsync(
      int eventId,
      int userId,
      CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id, int userId,
            CancellationToken cancellationToken = default);
    }
}