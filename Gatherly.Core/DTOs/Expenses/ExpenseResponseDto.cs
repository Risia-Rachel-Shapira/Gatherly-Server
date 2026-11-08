using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.Expenses
{
    public class ExpenseResponseDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int? ItemId { get; set; }

        public int PaidByUserId { get; set; }

        public string PaidByUserName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
