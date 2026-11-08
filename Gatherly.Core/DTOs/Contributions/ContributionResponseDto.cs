using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.Contributions
{
    public class ContributionResponseDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
