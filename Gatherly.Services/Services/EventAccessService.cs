using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class EventAccessService : IEventAccessService
    {
        private readonly IEventRepository _events;
        private readonly IEventMemberRepository _members;
        private readonly IUserRepository _users;

        public EventAccessService(
            IEventRepository events,
            IEventMemberRepository members,
            IUserRepository users)
        {
            _events = events;
            _members = members;
            _users = users;
        }

        public async Task<Event> EnsureAccessAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var eventEntity = await _events.GetByIdAsync(
                eventId, cancellationToken);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            var user = await _users.GetByIdAsync(
                userId, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException();

            // The Organizer always has access.
            if (eventEntity.OrganizerId == userId)
                return eventEntity;

            // Other users must be active members.
            var membership = await _members.GetByEventAndUserAsync(
                eventId, userId, cancellationToken);

            if (membership == null || !membership.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "You are not an active member of this event.");
            }

            return eventEntity;
        }
    }
}