using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<List<Item>> GetByEventAsync(
            int eventId,
            CancellationToken cancellationToken = default);
    }
}