using System.Security.Claims;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.API.Controllers.Events
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventMembersController : ControllerBase
    {
        private readonly IEventMemberService _memberService;

        public EventMembersController(IEventMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetActiveMembers(
            [FromRoute] int eventId,
            CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var result = await _memberService.GetActiveMembersAsync(
                eventId, userId, cancellationToken);

            return Ok(result);
        }
    }
}
