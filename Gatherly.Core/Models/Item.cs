using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.Models
{
    public class Item
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public Event Event { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Quantity { get; set; }

        public decimal? EstimatedCost { get; set; }

        public decimal? ActualCost { get; set; }

        public int AddedByUserId { get; set; }
        public Guid Version { get; set; } = Guid.NewGuid();

        public User AddedByUser { get; set; } = null!;
    }
}
