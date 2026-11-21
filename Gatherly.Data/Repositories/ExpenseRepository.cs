using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(AppDbContext context) : base(context) { }

        public async Task<List<Expense>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.PaidByUser)
                .Where(e => e.EventId == eventId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Expense?> GetByIdWithPaidByUserAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(e => e.PaidByUser)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

    }
}