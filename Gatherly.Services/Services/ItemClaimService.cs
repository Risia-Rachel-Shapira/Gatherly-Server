
using AutoMapper;
using Gatherly.Core.DTOs.ItemClaims;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class ItemClaimService : IItemClaimService
    {
        private readonly IItemClaimRepository _claims;
        private readonly IItemRepository _items;
        private readonly IMapper _mapper;
        private readonly IEventAccessService _eventAccess;

        public ItemClaimService(
            IItemClaimRepository claims,
            IItemRepository items,
            IMapper mapper,
            IEventAccessService eventAccess)
        {
            _claims = claims;
            _items = items;
            _mapper = mapper;
            _eventAccess = eventAccess;
        }
        public async Task<List<ItemClaimResponseDto>> GetByItemAsync(
    int itemId,
    int userId,
    CancellationToken cancellationToken = default)
        {
            var item = await _items.GetByIdAsync(
                itemId, cancellationToken)
                ?? throw new KeyNotFoundException("Item not found.");

            await _eventAccess.EnsureAccessAsync(
                item.EventId, userId, cancellationToken);

            var claims = await _claims.GetActiveClaimsWithUsersAsync(
                itemId, cancellationToken);

            return _mapper.Map<List<ItemClaimResponseDto>>(claims);
        }
        public async Task<ItemClaimResponseDto> CreateAsync(
            CreateItemClaimDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be positive.");

            var item = await _items.GetByIdAsync(
                dto.ItemId, cancellationToken)
                ?? throw new KeyNotFoundException("Item not found.");

            await _eventAccess.EnsureAccessAsync(
                item.EventId, userId, cancellationToken);

            var claims = await _claims.GetActiveClaimsAsync(
                dto.ItemId, cancellationToken);

            var claimedQuantity = claims.Sum(x => x.Quantity);

            if (claimedQuantity + dto.Quantity > item.Quantity)
                throw new InvalidOperationException(
                    "Not enough available quantity.");

            var claim = new ItemClaim
            {
                ItemId = dto.ItemId,
                UserId = userId,
                Quantity = dto.Quantity
            };

            await _claims.AddAsync(claim, cancellationToken);

            // Trigger the Item concurrency token.
            _items.Update(item);

            await _claims.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ItemClaimResponseDto>(claim);
        }

        public async Task<bool> UpdateAsync(
            int id,
            CreateItemClaimDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be positive.");

            var claim = await _claims.GetByIdAsync(
                id, cancellationToken);

            if (claim == null || !claim.IsActive)
                return false;

            if (claim.UserId != userId)
                throw new UnauthorizedAccessException();

            var item = await _items.GetByIdAsync(
                claim.ItemId, cancellationToken)
                ?? throw new KeyNotFoundException("Item not found.");

            await _eventAccess.EnsureAccessAsync(
                item.EventId, userId, cancellationToken);

            var claims = await _claims.GetActiveClaimsAsync(
                claim.ItemId, cancellationToken);

            var otherClaimsQuantity = claims
                .Where(x => x.Id != id)
                .Sum(x => x.Quantity);

            if (otherClaimsQuantity + dto.Quantity > item.Quantity)
                throw new InvalidOperationException(
                    "Not enough available quantity.");

            claim.Quantity = dto.Quantity;

            _claims.Update(claim);
            _items.Update(item);

            await _claims.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> CancelAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var claim = await _claims.GetByIdAsync(
                id, cancellationToken);

            if (claim == null || !claim.IsActive)
                return false;

            if (claim.UserId != userId)
                throw new UnauthorizedAccessException();

            var item = await _items.GetByIdAsync(
                claim.ItemId, cancellationToken)
                ?? throw new KeyNotFoundException("Item not found.");

            await _eventAccess.EnsureAccessAsync(
                item.EventId, userId, cancellationToken);

            claim.IsActive = false;

            _claims.Update(claim);
            _items.Update(item);

            await _claims.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
