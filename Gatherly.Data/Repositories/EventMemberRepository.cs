using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class EventMemberRepository
        : Repository<EventMember>, IEventMemberRepository
    {
        public EventMemberRepository(AppDbContext context)
            : base(context)
        {
        }

        public async Task<List<EventMember>> GetActiveMembersAsync(
          int eventId,
          CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.User)
                .Where(m => m.EventId == eventId && m.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<EventMember?> GetByEventAndUserAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(
                    m => m.EventId == eventId && m.UserId == userId,
                    cancellationToken);
        }
    }
}