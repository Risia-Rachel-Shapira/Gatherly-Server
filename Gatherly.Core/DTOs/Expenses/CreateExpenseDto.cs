using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.Expenses
{
    public class CreateExpenseDto
    {
        public int EventId { get; set; }

        public int? ItemId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }
}
