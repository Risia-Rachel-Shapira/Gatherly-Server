using Gatherly.Core.Exceptions;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Services.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _usersMock;
        private readonly PasswordHasher<User> _hasher;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _usersMock = new Mock<IUserRepository>();
            _hasher = new PasswordHasher<User>();
            _service = new UserService(_usersMock.Object, _hasher);
        }

        private User SetupUser(string email, string password)
        {
            var user = new User { Id = 1, Email = email };
            user.PasswordHash = _hasher.HashPassword(user, password);

            _usersMock
                .Setup(x => x.GetByEmailAsync(
                    email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            return user;
        }

        [Fact]
        public async Task ValidateCredentialsAsync_CorrectPassword_ReturnsUser()
        {
            var user = SetupUser("a@b.com", "Secret123!");

            var result = await _service.ValidateCredentialsAsync(
                "  A@B.com ", "Secret123!");

            Assert.Same(user, result);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull()
        {
            SetupUser("a@b.com", "Secret123!");

            var result = await _service.ValidateCredentialsAsync(
                "a@b.com", "Wrong123!");

            Assert.Null(result);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_UnknownEmail_ReturnsNull()
        {
            var result = await _service.ValidateCredentialsAsync(
                "nobody@b.com", "Secret123!");

            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ThrowsConflictException()
        {
            SetupUser("a@b.com", "Secret123!");

            await Assert.ThrowsAsync<ConflictException>(
                () => _service.RegisterAsync(new User
                {
                    Email = "a@b.com",
                    PasswordHash = "Other123!"
                }));

            _usersMock.Verify(
                x => x.AddAsync(
                    It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
