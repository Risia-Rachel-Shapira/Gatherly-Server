using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IItemClaimRepository : IRepository<ItemClaim>
    {
        Task<List<ItemClaim>> GetActiveClaimsAsync(
            int itemId,
            CancellationToken cancellationToken = default);

        // Sum of active claim quantities per item in an event,
        // keyed by ItemId, in one query. Items with no active
        // claims are not included.
        Task<Dictionary<int, int>> GetClaimedQuantitiesByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default);

        Task<List<ItemClaim>> GetActiveClaimsWithUsersAsync(
            int itemId,
            CancellationToken cancellationToken = default);

        // All claims of an item, active and inactive (tracked).
        Task<List<ItemClaim>> GetByItemAsync(
            int itemId,
            CancellationToken cancellationToken = default);
    }
}