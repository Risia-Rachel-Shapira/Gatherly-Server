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

        Task<JoinInvitationResult?> JoinAsync(
            string token,
            int userId,
            CancellationToken cancellationToken = default);
    }
}