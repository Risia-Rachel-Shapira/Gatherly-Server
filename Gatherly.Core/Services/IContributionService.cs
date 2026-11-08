using Gatherly.Core.DTOs.Contributions;

namespace Gatherly.Core.Services
{
    public interface IContributionService
    {
        Task<ContributionResponseDto> CreateAsync(
            CreateContributionDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<List<ContributionResponseDto>> GetByEventAsync(
    int eventId,
    int userId,
    CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            int id, CreateContributionDto dto, int userId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int id, int userId,
            CancellationToken cancellationToken = default);
    }
}