using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        public ItemRepository(AppDbContext context) : base(context) { }

        public async Task<List<Item>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.EventId == eventId)
                .ToListAsync(cancellationToken);
        }
    }
}