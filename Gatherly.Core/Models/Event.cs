using Gatherly.Core.Enums;

namespace Gatherly.Core.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public string? Location { get; set; }

        public decimal? Budget { get; set; }

        public int OrganizerId { get; set; }

        public User Organizer { get; set; } = null!;

        public EventManagementType ManagementType { get; set; }
            = EventManagementType.Both;

        public ICollection<EventMember> Members { get; set; }
            = new List<EventMember>();

        public ICollection<Item> Items { get; set; }
            = new List<Item>();

        public ICollection<Invitation> Invitations { get; set; }
            = new List<Invitation>();
    }
}