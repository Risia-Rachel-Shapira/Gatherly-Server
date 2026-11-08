using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gatherly.Data.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        private readonly AppDbContext _appDbContext;

        public EventRepository(AppDbContext context) : base(context)
        {
            _appDbContext = context;
        }

        public async Task<List<Event>> GetByOrganizerAsync(
            int organizerId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Event> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<Event> Items, int TotalCount)> GetPagedForUserAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(e =>
                    e.OrganizerId == userId ||
                    _appDbContext.Set<EventMember>().Any(m =>
                        m.EventId == e.Id &&
                        m.UserId == userId &&
                        m.IsActive));

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task DeleteEventWithRelatedDataAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default)
        {
            await using var transaction =
                await _appDbContext.Database.BeginTransactionAsync(
                    cancellationToken);

            var itemIds = await _appDbContext.Items
                .Where(i => i.EventId == eventEntity.Id)
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            var claims = await _appDbContext.ItemClaims
                .Where(c => itemIds.Contains(c.ItemId))
                .ToListAsync(cancellationToken);

            _appDbContext.ItemClaims.RemoveRange(claims);

            var expenses = await _appDbContext.Expenses
                .Where(e => e.EventId == eventEntity.Id)
                .ToListAsync(cancellationToken);

            _appDbContext.Expenses.RemoveRange(expenses);

            var contributions = await _appDbContext.Contributions
                .Where(c => c.EventId == eventEntity.Id)
                .ToListAsync(cancellationToken);

            _appDbContext.Contributions.RemoveRange(contributions);

            var invitations = await _appDbContext.Invitations
                .Where(i => i.EventId == eventEntity.Id)
                .ToListAsync(cancellationToken);

            _appDbContext.Invitations.RemoveRange(invitations);

            var members = await _appDbContext.EventMembers
                .Where(m => m.EventId == eventEntity.Id)
                .ToListAsync(cancellationToken);

            _appDbContext.EventMembers.RemoveRange(members);

            var items = await _appDbContext.Items
                .Where(i => i.EventId == eventEntity.Id)
                .ToListAsync(cancellationToken);

            _appDbContext.Items.RemoveRange(items);

            _appDbContext.Events.Remove(eventEntity);

            await _appDbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
    }
}