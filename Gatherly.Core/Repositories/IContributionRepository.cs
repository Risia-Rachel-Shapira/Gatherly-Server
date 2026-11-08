using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IContributionRepository : IRepository<Contribution>
    {
        Task<List<Contribution>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default);
    }
}