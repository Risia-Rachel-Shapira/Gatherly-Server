using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IEventMemberRepository : IRepository<EventMember>
    {
        Task<List<EventMember>> GetActiveMembersAsync(
            int eventId,
            CancellationToken cancellationToken = default);

        Task<EventMember?> GetByEventAndUserAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default);
    }
}