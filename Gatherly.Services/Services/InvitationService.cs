
using Gatherly.Core.DTOs.Invitations;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using System.Security.Cryptography;

namespace Gatherly.Services.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _invitations;
        private readonly IEventMemberRepository _members;
        private readonly IEventRepository _events;

        public InvitationService(
            IInvitationRepository invitations,
            IEventMemberRepository members,
            IEventRepository events)
        {
            _invitations = invitations;
            _members = members;
            _events = events;
        }

        public async Task<InvitationResponseDto> CreateAsync(
            CreateInvitationDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.EventId <= 0)
                throw new ArgumentException("Invalid event ID.");

            if (dto.ExpiresInDays < 1 || dto.ExpiresInDays > 30)
                throw new ArgumentException(
                    "Invitation expiration must be between 1 and 30 days.");

            var eventEntity = await _events.GetByIdAsync(
                dto.EventId, cancellationToken);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            // Only the Organizer can create invitations.
            if (eventEntity.OrganizerId != userId)
                throw new UnauthorizedAccessException(
                    "Only the event organizer can create invitations.");

            var invitation = new Invitation
            {
                EventId = dto.EventId,

                // Generate a secure, unique invitation token.
                Token = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(32))
                    .ToLowerInvariant(),

                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(dto.ExpiresInDays),
                IsActive = true
            };

            await _invitations.AddAsync(
                invitation, cancellationToken);

            await _invitations.SaveChangesAsync(
                cancellationToken);

            return new InvitationResponseDto
            {
                Id = invitation.Id,
                EventId = invitation.EventId,
                Token = invitation.Token,
                ExpiresAt = invitation.ExpiresAt,
                IsActive = invitation.IsActive
            };
        }

        public async Task<JoinInvitationResult?> JoinAsync(
      string token,
      int userId,
      CancellationToken cancellationToken = default)
        {
            var invitation = await _invitations.GetByTokenAsync(
                token, cancellationToken);

            if (invitation == null ||
                !invitation.IsActive ||
                invitation.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            var eventEntity = await _events.GetByIdAsync(
                invitation.EventId, cancellationToken);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            // The organizer is already part of the event.
            if (eventEntity.OrganizerId == userId)
            {
                return new JoinInvitationResult
                {
                    IsOrganizer = true,
                    Member = null
                };
            }

            var existingMember =
                await _members.GetByEventAndUserAsync(
                    invitation.EventId,
                    userId,
                    cancellationToken);

            if (existingMember != null && existingMember.IsActive)
            {
                return new JoinInvitationResult
                {
                    IsOrganizer = false,
                    Member = existingMember
                };
            }

            if (existingMember != null)
            {
                existingMember.IsActive = true;
                existingMember.Role = "Participant";
                existingMember.JoinedAt = DateTime.UtcNow;

                _members.Update(existingMember);

                await _members.SaveChangesAsync(cancellationToken);

                return new JoinInvitationResult
                {
                    IsOrganizer = false,
                    Member = existingMember
                };
            }

            var member = new EventMember
            {
                EventId = invitation.EventId,
                UserId = userId,
                Role = "Participant",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _members.AddAsync(member, cancellationToken);
            await _members.SaveChangesAsync(cancellationToken);

            return new JoinInvitationResult
            {
                IsOrganizer = false,
                Member = member
            };
        }
    }
}
