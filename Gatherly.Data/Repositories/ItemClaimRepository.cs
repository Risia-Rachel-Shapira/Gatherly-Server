using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class ItemClaimRepository
        : Repository<ItemClaim>, IItemClaimRepository
    {
        public ItemClaimRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<List<ItemClaim>> GetActiveClaimsAsync(
            int itemId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.ItemId == itemId && c.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ItemClaim>> GetActiveClaimsWithUsersAsync(
            int itemId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.ItemId == itemId && c.IsActive)
                .Include(c => c.User)
                .OrderBy(c => c.ClaimedAt)
                .ToListAsync(cancellationToken);
        }
    }
}