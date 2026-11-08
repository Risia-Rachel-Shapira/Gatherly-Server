
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Services.Services;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class InvitationServiceTests
    {
        private readonly Mock<IInvitationRepository> _invitationsMock;
        private readonly Mock<IEventMemberRepository> _membersMock;
        private readonly Mock<IEventRepository> _eventsMock;
        private readonly InvitationService _service;

        public InvitationServiceTests()
        {
            _invitationsMock = new Mock<IInvitationRepository>();
            _membersMock = new Mock<IEventMemberRepository>();
            _eventsMock = new Mock<IEventRepository>();

            _eventsMock
                .Setup(x => x.GetByIdAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event
                {
                    OrganizerId = 1
                });

            _service = new InvitationService(
                _invitationsMock.Object,
                _membersMock.Object,
                _eventsMock.Object);
        }

        [Fact]
        public async Task JoinAsync_ValidInvitation_CreatesMembership()
        {
            var invitation = new Invitation
            {
                Id = 1,
                EventId = 5,
                Token = "valid-token",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "valid-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invitation);

            _membersMock
                .Setup(x => x.GetByEventAndUserAsync(
                    5, 20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventMember?)null);

            _membersMock
                .Setup(x => x.AddAsync(
                    It.IsAny<EventMember>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _membersMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.JoinAsync("valid-token", 20);

            Assert.NotNull(result);
            Assert.False(result.IsOrganizer);
            Assert.NotNull(result.Member);
            Assert.Equal(5, result.Member.EventId);
            Assert.Equal(20, result.Member.UserId);
            Assert.Equal("Participant", result.Member.Role);
            Assert.True(result.Member.IsActive);

            _membersMock.Verify(
                x => x.AddAsync(
                    It.Is<EventMember>(m =>
                        m.EventId == 5 &&
                        m.UserId == 20 &&
                        m.IsActive),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _membersMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task JoinAsync_ExpiredInvitation_ReturnsNull()
        {
            var invitation = new Invitation
            {
                EventId = 5,
                Token = "expired-token",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-10)
            };

            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "expired-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invitation);

            var result = await _service.JoinAsync(
                "expired-token", 20);

            Assert.Null(result);

            _membersMock.Verify(
                x => x.AddAsync(
                    It.IsAny<EventMember>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task JoinAsync_InactiveInvitation_ReturnsNull()
        {
            var invitation = new Invitation
            {
                EventId = 5,
                Token = "inactive-token",
                IsActive = false,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "inactive-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invitation);

            var result = await _service.JoinAsync(
                "inactive-token", 20);

            Assert.Null(result);
        }

        [Fact]
        public async Task JoinAsync_AlreadyActiveMember_ReturnsExistingMembership()
        {
            var invitation = new Invitation
            {
                EventId = 5,
                Token = "valid-token",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            var existingMember = new EventMember
            {
                Id = 7,
                EventId = 5,
                UserId = 20,
                Role = "Participant",
                IsActive = true
            };

            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "valid-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invitation);

            _membersMock
                .Setup(x => x.GetByEventAndUserAsync(
                    5, 20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMember);

            var result = await _service.JoinAsync(
                "valid-token", 20);

            Assert.NotNull(result);
            Assert.False(result.IsOrganizer);
            Assert.Same(existingMember, result.Member);

            _membersMock.Verify(
                x => x.AddAsync(
                    It.IsAny<EventMember>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _membersMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task JoinAsync_InactiveExistingMember_ReactivatesMembership()
        {
            var invitation = new Invitation
            {
                EventId = 5,
                Token = "valid-token",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            var existingMember = new EventMember
            {
                Id = 7,
                EventId = 5,
                UserId = 20,
                Role = "Participant",
                IsActive = false
            };

            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "valid-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invitation);

            _membersMock
                .Setup(x => x.GetByEventAndUserAsync(
                    5, 20,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingMember);

            _membersMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.JoinAsync(
                "valid-token", 20);

            Assert.NotNull(result);
            Assert.False(result.IsOrganizer);
            Assert.NotNull(result.Member);
            Assert.True(result.Member.IsActive);
            Assert.Equal("Participant", result.Member.Role);

            _membersMock.Verify(
                x => x.Update(existingMember),
                Times.Once);

            _membersMock.Verify(
                x => x.AddAsync(
                    It.IsAny<EventMember>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _membersMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task JoinAsync_Organizer_ReturnsOrganizerResult()
        {
            var invitation = new Invitation
            {
                EventId = 5,
                Token = "organizer-token",
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "organizer-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invitation);

            var result = await _service.JoinAsync(
                "organizer-token", 1);

            Assert.NotNull(result);
            Assert.True(result.IsOrganizer);
            Assert.Null(result.Member);

            _membersMock.Verify(
                x => x.AddAsync(
                    It.IsAny<EventMember>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _membersMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task JoinAsync_NonExistingInvitation_ReturnsNull()
        {
            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "missing-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Invitation?)null);

            var result = await _service.JoinAsync(
                "missing-token", 20);

            Assert.Null(result);

            _membersMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
