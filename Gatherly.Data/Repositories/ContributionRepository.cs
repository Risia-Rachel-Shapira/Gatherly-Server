using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class ContributionRepository : Repository<Contribution>, IContributionRepository
    {
        public ContributionRepository(AppDbContext context) : base(context) { }

        public async Task<List<Contribution>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.EventId == eventId)
                .ToListAsync(cancellationToken);
        }
    }
}