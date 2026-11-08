namespace Gatherly.Core.DTOs.Events
{
    public class EventMemberResponseDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}