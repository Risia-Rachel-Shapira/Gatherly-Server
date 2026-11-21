using AutoMapper;
using Gatherly.Core.DTOs.Admin;
using Gatherly.Core.Enums;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Services.Mapping;
using Gatherly.Services.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IEventRepository> _eventRepositoryMock = new();
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            var mapper = new MapperConfiguration(
                cfg => cfg.AddProfile<MappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();

            _service = new AdminService(
                _userRepositoryMock.Object,
                _eventRepositoryMock.Object,
                mapper);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsPagedUsersWithRoles()
        {
            var users = new List<User>
            {
                new User { Id = 1, FullName = "Admin", Email = "admin@test", Role = UserRole.Admin },
                new User { Id = 2, FullName = "Regular", Email = "user@test", Role = UserRole.User }
            };

            _userRepositoryMock
                .Setup(r => r.GetPagedAsync(2, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((users, 5));

            var result = await _service.GetUsersAsync(2, 2);

            Assert.Equal(2, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Collection(result.Items,
                u => { Assert.Equal(1, u.Id); Assert.Equal("Admin", u.Role); },
                u => { Assert.Equal(2, u.Id); Assert.Equal("User", u.Role); });
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetUsersAsync_InvalidPaging_ThrowsArgumentException(
            int pageNumber, int pageSize)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetUsersAsync(pageNumber, pageSize));

            _userRepositoryMock.Verify(
                r => r.GetPagedAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetEventsAsync_ReturnsAllEventsPagedWithOrganizerNames()
        {
            var events = new List<Event>
            {
                new Event { Id = 1, Name = "Picnic", OrganizerId = 10,
                    Organizer = new User { Id = 10, FullName = "Dana" } },
                new Event { Id = 2, Name = "Party", OrganizerId = 20,
                    Organizer = new User { Id = 20, FullName = "Yossi" } }
            };

            _eventRepositoryMock
                .Setup(r => r.GetPagedAsync(1, 2, It.IsAny<CancellationToken>()))
                .ReturnsAsync((events, 3));

            var result = await _service.GetEventsAsync(1, 2);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Collection(result.Items,
                e => { Assert.Equal(1, e.Id); Assert.Equal("Dana", e.OrganizerName); },
                e => { Assert.Equal(2, e.Id); Assert.Equal("Yossi", e.OrganizerName); });

            // System-wide query, not the per-user one.
            _eventRepositoryMock.Verify(
                r => r.GetPagedForUserAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetEventsAsync_InvalidPaging_ThrowsArgumentException(
            int pageNumber, int pageSize)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetEventsAsync(pageNumber, pageSize));

            _eventRepositoryMock.Verify(
                r => r.GetPagedAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteEventAsync_EventOfAnotherOrganizer_DeletesWithRelatedData()
        {
            var eventEntity = new Event { Id = 7, OrganizerId = 42 };

            _eventRepositoryMock
                .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(eventEntity);

            var result = await _service.DeleteEventAsync(7);

            Assert.True(result);

            _eventRepositoryMock.Verify(
                r => r.DeleteEventWithRelatedDataAsync(
                    eventEntity, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteEventAsync_EventNotFound_ReturnsFalse()
        {
            _eventRepositoryMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Event?)null);

            var result = await _service.DeleteEventAsync(99);

            Assert.False(result);

            _eventRepositoryMock.Verify(
                r => r.DeleteEventWithRelatedDataAsync(
                    It.IsAny<Event>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
