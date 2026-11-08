using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.Models
{
    public class Invitation
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public string Token { get; set; } = Guid.NewGuid().ToString();

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
