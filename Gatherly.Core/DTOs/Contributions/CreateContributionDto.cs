using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.Contributions
{
    public class CreateContributionDto
    {
        public int EventId { get; set; }

        public decimal Amount { get; set; }
    }
}
