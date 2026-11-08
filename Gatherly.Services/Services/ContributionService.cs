
using AutoMapper;
using Gatherly.Core.DTOs.Contributions;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class ContributionService : IContributionService
    {
        private readonly IContributionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IEventAccessService _eventAccess;

        public ContributionService(
            IContributionRepository repository,
            IMapper mapper,
            IEventAccessService eventAccess)
        {
            _repository = repository;
            _mapper = mapper;
            _eventAccess = eventAccess;
        }

        public async Task<ContributionResponseDto> CreateAsync(
            CreateContributionDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            await _eventAccess.EnsureAccessAsync(
                dto.EventId, userId, cancellationToken);

            var contribution = _mapper.Map<Contribution>(dto);
            contribution.UserId = userId;

            await _repository.AddAsync(contribution, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ContributionResponseDto>(contribution);
        }

        public async Task<List<ContributionResponseDto>> GetByEventAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await _eventAccess.EnsureAccessAsync(
                eventId, userId, cancellationToken);

            var contributions = await _repository.GetByEventAsync(
                eventId, cancellationToken);

            return _mapper.Map<List<ContributionResponseDto>>(
                contributions);
        }

        public async Task<bool> UpdateAsync(
            int id,
            CreateContributionDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var contribution = await _repository.GetByIdAsync(
                id, cancellationToken);

            if (contribution == null)
                return false;

            await _eventAccess.EnsureAccessAsync(
                contribution.EventId, userId, cancellationToken);

            if (contribution.UserId != userId)
                throw new UnauthorizedAccessException();

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            contribution.Amount = dto.Amount;

            _repository.Update(contribution);

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var contribution = await _repository.GetByIdAsync(
                id, cancellationToken);

            if (contribution == null)
                return false;

            await _eventAccess.EnsureAccessAsync(
                contribution.EventId, userId, cancellationToken);

            if (contribution.UserId != userId)
                throw new UnauthorizedAccessException();

            _repository.Delete(contribution);

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
