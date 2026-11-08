namespace Gatherly.Core.DTOs.Invitations
{
    public class InvitationResponseDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; }
    }
}