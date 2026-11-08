namespace Gatherly.Core.DTOs.Expenses
{
    public class ExpenseShareResponseDto
    {
        public int EventId { get; set; }

        public decimal TotalExpenses { get; set; }

        public int ActiveMembersCount { get; set; }

        public decimal? SharePerParticipant { get; set; }
    }
}