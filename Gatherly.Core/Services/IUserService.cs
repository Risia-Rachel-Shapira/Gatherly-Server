using Gatherly.Core.Models;

namespace Gatherly.Core.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(
            int id, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(
            string email, CancellationToken cancellationToken = default);

        // Returns the user when the email and password match,
        // otherwise null (no hint about which part was wrong).
        Task<User?> ValidateCredentialsAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);

        Task<User> RegisterAsync(
            User user, CancellationToken cancellationToken = default);
    }
}