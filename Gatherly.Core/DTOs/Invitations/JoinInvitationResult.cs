using Gatherly.Core.Models;

namespace Gatherly.Core.DTOs.Invitations
{
    public class JoinInvitationResult
    {
        public bool IsOrganizer { get; set; }
        public EventMember? Member { get; set; }
    }
}