using Gatherly.Core.Models;

namespace Gatherly.Core.Repositories
{
    public interface IEventRepository : IRepository<Event>
    {
        Task<List<Event>> GetByOrganizerAsync(
            int organizerId,
            CancellationToken cancellationToken = default);

        Task<(List<Event> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(List<Event> Items, int TotalCount)> GetPagedForUserAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task DeleteEventWithRelatedDataAsync(
            Event eventEntity,
            CancellationToken cancellationToken = default);
        Task<Event?> GetByIdWithOrganizerAsync(
    int id,
    CancellationToken cancellationToken = default);
    }
}