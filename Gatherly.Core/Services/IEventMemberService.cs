using Gatherly.Core.DTOs.Events;

namespace Gatherly.Core.Services
{
    public interface IEventMemberService
    {
        Task<List<EventMemberResponseDto>> GetActiveMembersAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default);
    }
}
