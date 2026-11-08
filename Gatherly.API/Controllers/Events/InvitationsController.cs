
using Gatherly.Core.DTOs.Invitations;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gatherly.API.Controllers.Events
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvitationsController : ControllerBase
    {
        private readonly IInvitationRepository _repository;
        private readonly IInvitationService _invitationService;

        public InvitationsController(
            IInvitationRepository repository,
            IInvitationService invitationService)
        {
            _repository = repository;
            _invitationService = invitationService;
        }

        private int GetUserId()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out var id))
                throw new UnauthorizedAccessException();

            return id;
        }

        // Create invitation
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateInvitationDto dto,
            CancellationToken cancellationToken)
        {
            var invitation = await _invitationService.CreateAsync(
                dto,
                GetUserId(),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetByToken),
                new { token = invitation.Token },
                invitation);
        }

        // Get invitation by token
        [HttpGet("{token}")]
        public async Task<IActionResult> GetByToken(
            string token,
            CancellationToken cancellationToken)
        {
            var invitation = await _repository.GetByTokenAsync(
                token, cancellationToken);

            if (invitation == null ||
                !invitation.IsActive ||
                invitation.ExpiresAt <= DateTime.UtcNow)
            {
                return NotFound();
            }

            return Ok(new InvitationResponseDto
            {
                Id = invitation.Id,
                EventId = invitation.EventId,
                Token = invitation.Token,
                ExpiresAt = invitation.ExpiresAt,
                IsActive = invitation.IsActive
            });
        }

        [HttpPost("{token}/join")]
        public async Task<IActionResult> Join(
           string token,
           CancellationToken cancellationToken)
        {
            var result = await _invitationService.JoinAsync(
                token,
                GetUserId(),
                cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Invitation is invalid, inactive, or expired."
                });
            }

            if (result.IsOrganizer)
            {
                return Ok(new
                {
                    message = "You are already the organizer of this event.",
                    isOrganizer = true
                });
            }

            var member = result.Member!;

            return Ok(new
            {
                member.Id,
                member.EventId,
                member.UserId,
                member.Role,
                member.JoinedAt,
                member.IsActive,
                isOrganizer = false
            });
        }
    }
}
