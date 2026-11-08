using Gatherly.Core.DTOs.Items;

namespace Gatherly.Core.Services
{
    public interface IItemService
    {
        Task<ItemResponseDto?> GetByIdAsync(
    int id,
    int userId,
    CancellationToken cancellationToken = default);

        Task<List<ItemResponseDto>> GetByEventAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default);

        Task<ItemResponseDto> CreateAsync(
            CreateItemDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            int id, UpdateItemDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id, int userId,
            CancellationToken cancellationToken = default);
    }
}