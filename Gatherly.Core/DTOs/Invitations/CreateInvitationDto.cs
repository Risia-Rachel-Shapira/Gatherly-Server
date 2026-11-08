namespace Gatherly.Core.DTOs.Invitations
{
    public class CreateInvitationDto
    {
        public int EventId { get; set; }

        // Default expiration: 7 days
        public int ExpiresInDays { get; set; } = 7;
    }
}