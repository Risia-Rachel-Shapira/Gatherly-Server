using Gatherly.Core.DTOs.ItemClaims;

namespace Gatherly.Core.Services
{
    public interface IItemClaimService
    {
        Task<List<ItemClaimResponseDto>> GetByItemAsync(
            int itemId,
            int userId,
            CancellationToken cancellationToken = default);

        Task<ItemClaimResponseDto> CreateAsync(
            CreateItemClaimDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            int id,
            CreateItemClaimDto dto,
            int userId,
            CancellationToken cancellationToken = default);

        Task<bool> CancelAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default);
    }
}