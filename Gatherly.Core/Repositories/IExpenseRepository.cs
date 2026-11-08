using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<List<Expense>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default);
    }
}