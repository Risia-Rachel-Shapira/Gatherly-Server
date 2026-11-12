using AutoMapper;
using Gatherly.Core.DTOs.Common;
using Gatherly.Core.DTOs.Events;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEventAccessService _eventAccess;
        private readonly IMapper _mapper;

        public EventService(
    IEventRepository eventRepository,
    IUserRepository userRepository,
    IEventAccessService eventAccess,
    IMapper mapper)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _eventAccess = eventAccess;
            _mapper = mapper;
        }

        public async Task<EventResponseDto?> GetByIdAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
        {
            await _eventAccess.EnsureAccessAsync(
                id, userId, cancellationToken);

            var eventEntity = await _eventRepository.GetByIdWithOrganizerAsync(
                id, cancellationToken);

            if (eventEntity == null)
                return null;

            var result = _mapper.Map<EventResponseDto>(eventEntity);

            result.OrganizerName = eventEntity.Organizer.FullName;

            return result;
        }

        public async Task<PagedResult<EventResponseDto>> GetAllAsync(
            int userId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentException(
                    "Page number must be at least 1.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100.");

            var (events, totalCount) =
                await _eventRepository.GetPagedForUserAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    cancellationToken);

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

        public async Task<EventResponseDto> CreateAsync(
            CreateEventDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(
                userId, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException();

            var eventEntity = _mapper.Map<Event>(dto);

            eventEntity.Date = DateTime.SpecifyKind(
                eventEntity.Date,
                DateTimeKind.Utc);

            eventEntity.OrganizerId = userId;
            eventEntity.Organizer = user;

            await _eventRepository.AddAsync(
                eventEntity, cancellationToken);

            await _eventRepository.SaveChangesAsync(
                cancellationToken);

            return _mapper.Map<EventResponseDto>(eventEntity);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateEventDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(
                id, cancellationToken);

            if (eventEntity == null)
                return false;

            EnsureOrganizer(eventEntity, userId);

            _mapper.Map(dto, eventEntity);

            _eventRepository.Update(eventEntity);

            await _eventRepository.SaveChangesAsync(
                cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(
                id, cancellationToken);

            if (eventEntity == null)
                return false;

            EnsureOrganizer(eventEntity, userId);

            await _eventRepository.DeleteEventWithRelatedDataAsync(
                eventEntity, cancellationToken);

            return true;
        }

        private static void EnsureOrganizer(
            Event eventEntity,
            int userId)
        {
            if (eventEntity.OrganizerId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Only the event organizer can manage this event.");
            }
        }
    }
}