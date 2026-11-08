using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.Items
{
    public class CreateItemDto
    {
        public int EventId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Quantity { get; set; }

        public decimal? EstimatedCost { get; set; }
    }
}
