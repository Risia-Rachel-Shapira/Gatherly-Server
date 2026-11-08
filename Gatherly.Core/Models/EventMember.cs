using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.Models
{
    public class EventMember
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public string Role { get; set; } = "Participant";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
