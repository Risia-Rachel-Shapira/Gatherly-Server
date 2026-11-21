using AutoMapper;
using Gatherly.Core.DTOs.Events;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class EventMemberService : IEventMemberService
    {
        private readonly IEventMemberRepository _members;
        private readonly IEventAccessService _eventAccess;
        private readonly IMapper _mapper;

        public EventMemberService(
            IEventMemberRepository members,
            IEventAccessService eventAccess,
            IMapper mapper)
        {
            _members = members;
            _eventAccess = eventAccess;
            _mapper = mapper;
        }

        public async Task<List<EventMemberResponseDto>> GetActiveMembersAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await _eventAccess.EnsureAccessAsync(
                eventId, userId, cancellationToken);

            var members = await _members.GetActiveMembersAsync(
                eventId, cancellationToken);

            return _mapper.Map<List<EventMemberResponseDto>>(members);
        }
    }
}
