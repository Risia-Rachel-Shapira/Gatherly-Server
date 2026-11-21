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

        public async Task<Dictionary<int, int>> GetClaimedQuantitiesByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.Item.EventId == eventId && c.IsActive)
                .GroupBy(c => c.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    Claimed = g.Sum(c => c.Quantity)
                })
                .ToDictionaryAsync(
                    x => x.ItemId,
                    x => x.Claimed,
                    cancellationToken);
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