
using AutoMapper;
using Gatherly.Core.DTOs.ItemClaims;
using Gatherly.Core.Exceptions;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Microsoft.Extensions.Logging;

namespace Gatherly.Services.Services
{
    public class ItemClaimService : IItemClaimService
    {
        private readonly IItemClaimRepository _claims;
        private readonly IItemRepository _items;
        private readonly IMapper _mapper;
        private readonly IEventAccessService _eventAccess;
        private readonly ILogger<ItemClaimService> _logger;

        public ItemClaimService(
            IItemClaimRepository claims,
            IItemRepository items,
            IMapper mapper,
            IEventAccessService eventAccess,
            ILogger<ItemClaimService> logger)
        {
            _claims = claims;
            _items = items;
            _mapper = mapper;
            _eventAccess = eventAccess;
            _logger = logger;
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
            {
                _logger.LogWarning(
                    "Claim rejected: user {UserId} requested {Requested} of item {ItemId}, " +
                    "only {Available} of {Total} available.",
                    userId, dto.Quantity, dto.ItemId,
                    item.Quantity - claimedQuantity, item.Quantity);

                throw new ConflictException(
                    "Not enough available quantity.");
            }

            var claim = new ItemClaim
            {
                ItemId = dto.ItemId,
                UserId = userId,
                Quantity = dto.Quantity
            };

            await _claims.AddAsync(claim, cancellationToken);

            // Trigger the Item concurrency token.
            _items.Update(item);

            await SaveWithConflictLoggingAsync(
                "create", dto.ItemId, userId, cancellationToken);

            _logger.LogInformation(
                "Claim {ClaimId} created: user {UserId} claimed {Quantity} of item {ItemId}.",
                claim.Id, userId, claim.Quantity, claim.ItemId);

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
            {
                _logger.LogWarning(
                    "Claim update rejected: claim {ClaimId}, user {UserId} requested {Requested} " +
                    "of item {ItemId}, only {Available} of {Total} available.",
                    id, userId, dto.Quantity, claim.ItemId,
                    item.Quantity - otherClaimsQuantity, item.Quantity);

                throw new ConflictException(
                    "Not enough available quantity.");
            }

            claim.Quantity = dto.Quantity;

            _claims.Update(claim);
            _items.Update(item);

            await SaveWithConflictLoggingAsync(
                "update", claim.ItemId, userId, cancellationToken);

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

            await SaveWithConflictLoggingAsync(
                "cancel", claim.ItemId, userId, cancellationToken);

            return true;
        }

        // Saves and logs a Warning when another user changed the item
        // first (optimistic concurrency conflict), then rethrows.
        private async Task SaveWithConflictLoggingAsync(
            string operation,
            int itemId,
            int userId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _claims.SaveChangesAsync(cancellationToken);
            }
            catch (ConflictException)
            {
                _logger.LogWarning(
                    "Concurrency conflict on claim {Operation}: item {ItemId}, user {UserId}.",
                    operation, itemId, userId);

                throw;
            }
        }
    }
}
