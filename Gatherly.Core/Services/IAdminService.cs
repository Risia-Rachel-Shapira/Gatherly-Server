using Gatherly.Core.DTOs.Admin;
using Gatherly.Core.DTOs.Common;
using Gatherly.Core.DTOs.Events;

namespace Gatherly.Core.Services
{
    public interface IAdminService
    {
        Task<PagedResult<AdminUserDto>> GetUsersAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        // All events in the system, not only the admin's own.
        Task<PagedResult<EventResponseDto>> GetEventsAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        // Deletes any event with its related data, even when the
        // admin is not its organizer. Returns false if not found.
        Task<bool> DeleteEventAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}
