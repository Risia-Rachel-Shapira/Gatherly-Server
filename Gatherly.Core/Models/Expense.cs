using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public int? ItemId { get; set; }

        public Item? Item { get; set; }

        public int PaidByUserId { get; set; }

        public User PaidByUser { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
