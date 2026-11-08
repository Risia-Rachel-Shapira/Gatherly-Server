
using System.Security.Claims;
using AutoMapper;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Gatherly.Core.DTOs.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.API.Controllers.Events
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventMembersController : ControllerBase
    {
        private readonly IEventMemberRepository _repository;
        private readonly IEventAccessService _eventAccess;
        private readonly IMapper _mapper;

        public EventMembersController(
            IEventMemberRepository repository,
            IEventAccessService eventAccess,
            IMapper mapper)
        {
            _repository = repository;
            _eventAccess = eventAccess;
            _mapper = mapper;
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetActiveMembers(
            int eventId,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            await _eventAccess.EnsureAccessAsync(
                eventId, userId, cancellationToken);

            var members = await _repository.GetActiveMembersAsync(
                eventId, cancellationToken);

            var result =
                _mapper.Map<List<EventMemberResponseDto>>(members);

            return Ok(result);
        }
    }
}
