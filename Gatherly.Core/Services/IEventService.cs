using Gatherly.Core.DTOs.Common;
using Gatherly.Core.DTOs.Events;

namespace Gatherly.Core.Services
{
    public interface IEventService
    {
        Task<EventResponseDto?> GetByIdAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default);

        Task<PagedResult<EventResponseDto>> GetAllAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<EventResponseDto> CreateAsync(
            CreateEventDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            int id, UpdateEventDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id, int userId,
            CancellationToken cancellationToken = default);
    }
}