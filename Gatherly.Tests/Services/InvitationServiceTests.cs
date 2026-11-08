
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
        private readonly InvitationService _service;
        private readonly Mock<IEventRepository> _eventsMock;

        public InvitationServiceTests()
        {
            _invitationsMock = new Mock<IInvitationRepository>();
            _membersMock = new Mock<IEventMemberRepository>();
            _eventsMock = new Mock<IEventRepository>();

            _service = new InvitationService(
                _invitationsMock.Object,
                _membersMock.Object,
                _eventsMock.Object);
        }

        [Fact]
        public async Task JoinAsync_ValidInvitation_CreatesMembership()
        {
            // Arrange
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

            // Act
            var result = await _service.JoinAsync("valid-token", 20);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.EventId);
            Assert.Equal(20, result.UserId);
            Assert.Equal("Participant", result.Role);
            Assert.True(result.IsActive);

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
            // Arrange
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

            // Act
            var result = await _service.JoinAsync(
                "expired-token", 20);

            // Assert
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
            // Arrange
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

            // Act
            var result = await _service.JoinAsync(
                "inactive-token", 20);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task JoinAsync_AlreadyActiveMember_ReturnsExistingMembership()
        {
            // Arrange
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

            // Act
            var result = await _service.JoinAsync(
                "valid-token", 20);

            // Assert
            Assert.Same(existingMember, result);

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
            // Arrange
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

            // Act
            var result = await _service.JoinAsync(
                "valid-token", 20);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsActive);
            Assert.Equal("Participant", result.Role);

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
        public async Task JoinAsync_NonExistingInvitation_ReturnsNull()
        {
            // Arrange
            _invitationsMock
                .Setup(x => x.GetByTokenAsync(
                    "missing-token",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Invitation?)null);

            // Act
            var result = await _service.JoinAsync(
                "missing-token", 20);

            // Assert
            Assert.Null(result);

            _membersMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
