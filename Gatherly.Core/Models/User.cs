using Gatherly.Core.Enums;

namespace Gatherly.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EventMember> EventMemberships { get; set; }
            = new List<EventMember>();
    }
}