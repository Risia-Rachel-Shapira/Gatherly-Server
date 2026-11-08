using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.ItemClaims
{
    public class ItemClaimResponseDto
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTime ClaimedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
