using Gatherly.Core.DTOs.Expenses;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class ExpenseShareService : IExpenseShareService
    {
        private readonly IExpenseRepository _expenses;
        private readonly IEventMemberRepository _members;
        private readonly IEventAccessService _eventAccess;

        public ExpenseShareService(
            IExpenseRepository expenses,
            IEventMemberRepository members,
            IEventAccessService eventAccess)
        {
            _expenses = expenses;
            _members = members;
            _eventAccess = eventAccess;
        }

        public async Task<ExpenseShareResponseDto> GetShareAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await _eventAccess.EnsureAccessAsync(
                eventId, userId, cancellationToken);

            var expenses = await _expenses.GetByEventAsync(
                eventId, cancellationToken);

            var members = await _members.GetActiveMembersAsync(
                eventId, cancellationToken);

            var totalExpenses = expenses.Sum(e => e.Amount);
            var membersCount = members.Count;

            return new ExpenseShareResponseDto
            {
                EventId = eventId,
                TotalExpenses = totalExpenses,
                ActiveMembersCount = membersCount,
                SharePerParticipant = membersCount == 0
                    ? null
                    : totalExpenses / membersCount
            };
        }
    }
}