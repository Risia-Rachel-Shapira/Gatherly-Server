using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Identity;

namespace Gatherly.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            IUserRepository repository,
            IPasswordHasher<User> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> GetByIdAsync(
            int id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(
            string email, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<User> RegisterAsync(
            User user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _repository.GetByEmailAsync(
                user.Email, cancellationToken);

            if (existingUser != null)
                throw new InvalidOperationException("Email already exists.");

            user.PasswordHash =
                _passwordHasher.HashPassword(user, user.PasswordHash);

            await _repository.AddAsync(user, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}