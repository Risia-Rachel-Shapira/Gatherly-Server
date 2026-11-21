using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<(List<User> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}