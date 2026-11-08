using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.Models
{
    public class ItemClaim
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int Quantity { get; set; }

        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
