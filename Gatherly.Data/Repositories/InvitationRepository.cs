using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class InvitationRepository : Repository<Invitation>, IInvitationRepository
    {
        public InvitationRepository(AppDbContext context) : base(context) { }

        public async Task<Invitation?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
        }
    }
}