using AutoMapper;
using Gatherly.Core.DTOs.Admin;
using Gatherly.Core.DTOs.Common;
using Gatherly.Core.DTOs.Events;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public AdminService(
            IUserRepository userRepository,
            IEventRepository eventRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<AdminUserDto>> GetUsersAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ValidatePaging(pageNumber, pageSize);

            var (users, totalCount) = await _userRepository.GetPagedAsync(
                pageNumber, pageSize, cancellationToken);

            return new PagedResult<AdminUserDto>
            {
                Items = _mapper.Map<List<AdminUserDto>>(users),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResult<EventResponseDto>> GetEventsAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ValidatePaging(pageNumber, pageSize);

            var (events, totalCount) = await _eventRepository.GetPagedAsync(
                pageNumber, pageSize, cancellationToken);

            return new PagedResult<EventResponseDto>
            {
                Items = events.Select(e =>
                {
                    var dto = _mapper.Map<EventResponseDto>(e);
                    dto.OrganizerName = e.Organizer.FullName;
                    return dto;
                }).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> DeleteEventAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(
                id, cancellationToken);

            if (eventEntity == null)
                return false;

            // No organizer check: access is limited to the Admin role
            // by the controller. Same cascade as an organizer delete.
            await _eventRepository.DeleteEventWithRelatedDataAsync(
                eventEntity, cancellationToken);

            return true;
        }

        private static void ValidatePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException(
                    "Page number must be at least 1.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100.");
        }
    }
}
