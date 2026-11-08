using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.Models
{
    public class Contribution
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
