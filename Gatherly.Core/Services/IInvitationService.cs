using Gatherly.Core.DTOs.Invitations;
using Gatherly.Core.Models;

namespace Gatherly.Core.Services
{
    public interface IInvitationService
    {
        Task<InvitationResponseDto> CreateAsync(
            CreateInvitationDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        // Returns the invitation if it exists, is active and has not
        // expired; otherwise null.
        Task<InvitationResponseDto?> GetByTokenAsync(
            string token,
            CancellationToken cancellationToken = default);

        Task<JoinInvitationResult?> JoinAsync(
            string token,
            int userId,
            CancellationToken cancellationToken = default);
    }
}