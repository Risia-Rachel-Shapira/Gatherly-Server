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
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            var mapper = new MapperConfiguration(
                cfg => cfg.AddProfile<MappingProfile>(),
                NullLoggerFactory.Instance).CreateMapper();

            _service = new AdminService(_userRepositoryMock.Object, mapper);
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
    }
}
