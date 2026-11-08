using Gatherly.Core.Models;

namespace Gatherly.Core.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(
            int id, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(
            string email, CancellationToken cancellationToken = default);

        Task<User> RegisterAsync(
            User user, CancellationToken cancellationToken = default);
    }
}