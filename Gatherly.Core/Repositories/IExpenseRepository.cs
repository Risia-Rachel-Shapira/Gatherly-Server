using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<List<Expense>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default);

        Task<Expense?> GetByIdWithPaidByUserAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<bool> AnyByItemAsync(
            int itemId,
            CancellationToken cancellationToken = default);
    }
}