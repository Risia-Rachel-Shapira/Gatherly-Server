using Gatherly.Core.DTOs.Expenses;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class ExpenseShareService : IExpenseShareService
    {
        private readonly IExpenseRepository _expenses;
        private readonly IEventMemberRepository _members;
        private readonly IEventRepository _events;
        private readonly IEventAccessService _eventAccess;

        public ExpenseShareService(
            IExpenseRepository expenses,
            IEventMemberRepository members,
            IEventRepository events,
            IEventAccessService eventAccess)
        {
            _expenses = expenses;
            _members = members;
            _events = events;
            _eventAccess = eventAccess;
        }

        public async Task<ExpenseShareResponseDto> GetShareAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await _eventAccess.EnsureAccessAsync(
                eventId, userId, cancellationToken);

            var eventEntity = await _events.GetByIdAsync(
                eventId, cancellationToken);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            var expenses = await _expenses.GetByEventAsync(
                eventId, cancellationToken);

            var members = await _members.GetActiveMembersAsync(
                eventId, cancellationToken);

            // סופרים משתמשים ייחודיים, כולל מארגן האירוע.
            var participantIds = members
                .Select(m => m.UserId)
                .ToHashSet();

            participantIds.Add(eventEntity.OrganizerId);

            var membersCount = participantIds.Count;
            var totalExpenses = expenses.Sum(e => e.Amount);

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