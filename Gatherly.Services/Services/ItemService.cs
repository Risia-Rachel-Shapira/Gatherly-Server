
using AutoMapper;
using Gatherly.Core.DTOs.Items;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly IItemClaimRepository _claimRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventMemberRepository _memberRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ItemService(
            IItemRepository repository,
            IItemClaimRepository claimRepository,
            IEventRepository eventRepository,
            IEventMemberRepository memberRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _repository = repository;
            _claimRepository = claimRepository;
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ItemResponseDto?> GetByIdAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetByIdAsync(
                id, cancellationToken);

            if (item == null)
                return null;

            await EnsureEventAccessAsync(
                item.EventId, userId, cancellationToken);

            var dto = _mapper.Map<ItemResponseDto>(item);

            var claims = await _claimRepository.GetActiveClaimsAsync(
                id, cancellationToken);

            dto.ClaimedQuantity = claims.Sum(c => c.Quantity);
            dto.RemainingQuantity =
                item.Quantity - dto.ClaimedQuantity;

            return dto;
        }

        public async Task<List<ItemResponseDto>> GetByEventAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await EnsureEventAccessAsync(
                eventId, userId, cancellationToken);

            var items = await _repository.GetByEventAsync(
                eventId, cancellationToken);

            var result = new List<ItemResponseDto>();

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dto = _mapper.Map<ItemResponseDto>(item);

                var claims = await _claimRepository.GetActiveClaimsAsync(
                    item.Id, cancellationToken);

                dto.ClaimedQuantity = claims.Sum(c => c.Quantity);
                dto.RemainingQuantity =
                    item.Quantity - dto.ClaimedQuantity;

                result.Add(dto);
            }

            return result;
        }

        public async Task<ItemResponseDto> CreateAsync(
            CreateItemDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

            // Organizer or active Participant can add an item.
            await EnsureEventAccessAsync(
                dto.EventId, userId, cancellationToken);

            var item = _mapper.Map<Item>(dto);
            item.AddedByUserId = userId;

            await _repository.AddAsync(item, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var response = _mapper.Map<ItemResponseDto>(item);

            response.ClaimedQuantity = 0;
            response.RemainingQuantity = item.Quantity;

            return response;
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateItemDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetByIdAsync(
                id, cancellationToken);

            if (item == null)
                return false;

            await EnsureCanManageItemAsync(
                item, userId, cancellationToken);

            if (dto.Quantity <= 0)
                throw new ArgumentException(
                    "Quantity must be greater than zero.");

            var claims = await _claimRepository.GetActiveClaimsAsync(
                id, cancellationToken);

            if (claims.Sum(c => c.Quantity) > dto.Quantity)
            {
                throw new InvalidOperationException(
                    "Quantity cannot be lower than the claimed quantity.");
            }

            // Preserve the original event and item owner.
            var originalEventId = item.EventId;
            var originalAddedByUserId = item.AddedByUserId;

            _mapper.Map(dto, item);

            item.EventId = originalEventId;
            item.AddedByUserId = originalAddedByUserId;

            _repository.Update(item);

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var item = await _repository.GetByIdAsync(
                id, cancellationToken);

            if (item == null)
                return false;

            await EnsureCanManageItemAsync(
                item, userId, cancellationToken);

            _repository.Delete(item);

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task<Event> EnsureEventAccessAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(
                eventId, cancellationToken);

            if (eventEntity == null)
                throw new KeyNotFoundException("Event not found.");

            var user = await _userRepository.GetByIdAsync(
                userId, cancellationToken);

            if (user == null)
                throw new UnauthorizedAccessException();

            // The Organizer always has access to their event.
            if (eventEntity.OrganizerId == userId)
                return eventEntity;

            // Other users must be active members.
            var membership =
                await _memberRepository.GetByEventAndUserAsync(
                    eventId, userId, cancellationToken);

            if (membership == null || !membership.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "You are not an active member of this event.");
            }

            return eventEntity;
        }        private async Task EnsureCanManageItemAsync(
            Item item,
            int userId,
            CancellationToken cancellationToken)
        {
            var eventEntity = await EnsureEventAccessAsync(
                item.EventId, userId, cancellationToken);

            // Organizer can manage all items in their event.
            if (eventEntity.OrganizerId == userId)
                return;

            // Participant can manage only their own items.
            if (item.AddedByUserId == userId)
                return;

            throw new UnauthorizedAccessException(
                "Participants can manage only items they added.");
        }
    }
}
