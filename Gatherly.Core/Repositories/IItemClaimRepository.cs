using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IItemClaimRepository : IRepository<ItemClaim>
    {
        Task<List<ItemClaim>> GetActiveClaimsAsync(
            int itemId,
            CancellationToken cancellationToken = default);

        Task<List<ItemClaim>> GetActiveClaimsWithUsersAsync(
            int itemId,
            CancellationToken cancellationToken = default);
    }
}