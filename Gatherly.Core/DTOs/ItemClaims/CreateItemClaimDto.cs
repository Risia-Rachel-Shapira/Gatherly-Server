using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.ItemClaims
{
    public class CreateItemClaimDto
    {
        public int ItemId { get; set; }

        public int Quantity { get; set; }
    }
}
