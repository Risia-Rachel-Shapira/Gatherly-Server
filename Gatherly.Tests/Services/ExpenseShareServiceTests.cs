
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Gatherly.Services.Services;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class ExpenseShareServiceTests
    {
        private readonly Mock<IExpenseRepository> _expensesMock;
        private readonly Mock<IEventMemberRepository> _membersMock;
        private readonly Mock<IEventRepository> _eventsMock;
        private readonly Mock<IEventAccessService> _eventAccessMock;
        private readonly ExpenseShareService _service;

        public ExpenseShareServiceTests()
        {
            _expensesMock = new Mock<IExpenseRepository>();
            _membersMock = new Mock<IEventMemberRepository>();
            _eventsMock = new Mock<IEventRepository>();
            _eventAccessMock = new Mock<IEventAccessService>();

            _eventAccessMock
                .Setup(x => x.EnsureAccessAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event());

            _eventsMock
                .Setup(x => x.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event
                {
                    Id = 1,
                    OrganizerId = 10
                });

            _service = new ExpenseShareService(
                _expensesMock.Object,
                _membersMock.Object,
                _eventsMock.Object,
                _eventAccessMock.Object);
        }

        [Fact]
        public async Task GetShareAsync_WithExpensesAndMembers_CalculatesCorrectShare()
        {
            var expenses = new List<Expense>
            {
                new Expense { Amount = 100 },
                new Expense { Amount = 200 },
                new Expense { Amount = 100 }
            };

            var members = new List<EventMember>
            {
                new EventMember { UserId = 20, IsActive = true },
                new EventMember { UserId = 30, IsActive = true },
                new EventMember { UserId = 40, IsActive = true },
                new EventMember { UserId = 50, IsActive = true }
            };

            _expensesMock
                .Setup(x => x.GetByEventAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expenses);

            _membersMock
                .Setup(x => x.GetActiveMembersAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(members);

            var result = await _service.GetShareAsync(1, 10);

            Assert.Equal(1, result.EventId);
            Assert.Equal(400m, result.TotalExpenses);
            Assert.Equal(5, result.ActiveMembersCount);
            Assert.Equal(80m, result.SharePerParticipant);
        }

        [Fact]
        public async Task GetShareAsync_NoActiveMembers_IncludesOrganizer()
        {
            _expensesMock
                .Setup(x => x.GetByEventAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Expense>
                {
                    new Expense { Amount = 300 }
                });

            _membersMock
                .Setup(x => x.GetActiveMembersAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventMember>());

            var result = await _service.GetShareAsync(1, 10);

            Assert.Equal(300m, result.TotalExpenses);
            Assert.Equal(1, result.ActiveMembersCount);
            Assert.Equal(300m, result.SharePerParticipant);
        }

        [Fact]
        public async Task GetShareAsync_OrganizerAlreadyInMembers_IsNotCountedTwice()
        {
            _membersMock
                .Setup(x => x.GetActiveMembersAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventMember>
                {
                    new EventMember { UserId = 10, IsActive = true },
                    new EventMember { UserId = 20, IsActive = true }
                });

            _expensesMock
                .Setup(x => x.GetByEventAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Expense>
                {
                    new Expense { Amount = 600 }
                });

            var result = await _service.GetShareAsync(1, 10);

            Assert.Equal(2, result.ActiveMembersCount);
            Assert.Equal(300m, result.SharePerParticipant);
        }

        [Fact]
        public async Task GetShareAsync_NoExpenses_ReturnsZeroShare()
        {
            _expensesMock
                .Setup(x => x.GetByEventAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Expense>());

            _membersMock
                .Setup(x => x.GetActiveMembersAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventMember>
                {
                    new EventMember { UserId = 20, IsActive = true },
                    new EventMember { UserId = 30, IsActive = true }
                });

            var result = await _service.GetShareAsync(1, 10);

            Assert.Equal(0m, result.TotalExpenses);
            Assert.Equal(3, result.ActiveMembersCount);
            Assert.Equal(0m, result.SharePerParticipant);
        }

        [Fact]
        public async Task GetShareAsync_WhenUserHasNoAccess_ThrowsUnauthorizedAccessException()
        {
            _eventAccessMock
                .Setup(x => x.EnsureAccessAsync(
                    1, 99, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetShareAsync(1, 99));

            _expensesMock.Verify(
                x => x.GetByEventAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
