using Gatherly.Core.Models;

namespace Gatherly.Core.Services
{
    public interface IEventAccessService
    {
        Task<Event> EnsureAccessAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default);
    }
}