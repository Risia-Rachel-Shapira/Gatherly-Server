
using AutoMapper;
using Gatherly.Core.DTOs.ItemClaims;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Gatherly.Services.Services;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class ItemClaimServiceTests
    {
        private readonly Mock<IItemClaimRepository> _claimsMock;
        private readonly Mock<IItemRepository> _itemsMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ItemClaimService _service;
        private readonly Mock<IEventAccessService> _eventAccessMock;

        public ItemClaimServiceTests()
        {
            _claimsMock = new Mock<IItemClaimRepository>();
            _itemsMock = new Mock<IItemRepository>();
            _mapperMock = new Mock<IMapper>();
            _eventAccessMock = new Mock<IEventAccessService>();

            _eventAccessMock
                .Setup(x => x.EnsureAccessAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event());

            _service = new ItemClaimService(
                _claimsMock.Object,
                _itemsMock.Object,
                _mapperMock.Object,
                _eventAccessMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidClaim_CreatesSuccessfully()
        {
            // Arrange
            var dto = new CreateItemClaimDto
            {
                ItemId = 1,
                Quantity = 2
            };

            const int userId = 10;

            var item = new Item
            {
                Id = 1,
                Quantity = 5
            };

            var response = new ItemClaimResponseDto
            {
                Id = 1,
                ItemId = 1,
                UserId = userId,
                Quantity = 2,
                IsActive = true
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            _claimsMock
                .Setup(x => x.GetActiveClaimsAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemClaim>());

            _claimsMock
                .Setup(x => x.AddAsync(
                    It.IsAny<ItemClaim>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _claimsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(x => x.Map<ItemClaimResponseDto>(
                    It.IsAny<ItemClaim>()))
                .Returns(response);

            // Act
            var result = await _service.CreateAsync(dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(2, result.Quantity);

            _claimsMock.Verify(
                x => x.AddAsync(
                    It.Is<ItemClaim>(c =>
                        c.ItemId == 1 &&
                        c.UserId == userId &&
                        c.Quantity == 2),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _itemsMock.Verify(
                x => x.Update(item),
                Times.Once);

            _claimsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidQuantity_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateItemClaimDto
            {
                ItemId = 1,
                Quantity = 0
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(dto, 10));

            _claimsMock.Verify(
                x => x.AddAsync(
                    It.IsAny<ItemClaim>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ItemNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new CreateItemClaimDto
            {
                ItemId = 999,
                Quantity = 1
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Item?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateAsync(dto, 10));
        }

        [Fact]
        public async Task CreateAsync_InsufficientQuantity_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new CreateItemClaimDto
            {
                ItemId = 1,
                Quantity = 4
            };

            var item = new Item
            {
                Id = 1,
                Quantity = 5
            };

            var existingClaims = new List<ItemClaim>
            {
                new ItemClaim
                {
                    Id = 1,
                    ItemId = 1,
                    UserId = 20,
                    Quantity = 2,
                    IsActive = true
                }
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            _claimsMock
                .Setup(x => x.GetActiveClaimsAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClaims);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(dto, 10));

            _claimsMock.Verify(
                x => x.AddAsync(
                    It.IsAny<ItemClaim>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidUpdate_UpdatesSuccessfully()
        {
            // Arrange
            const int claimId = 5;
            const int userId = 10;

            var dto = new CreateItemClaimDto
            {
                ItemId = 1,
                Quantity = 3
            };

            var claim = new ItemClaim
            {
                Id = claimId,
                ItemId = 1,
                UserId = userId,
                Quantity = 1,
                IsActive = true
            };

            var item = new Item
            {
                Id = 1,
                Quantity = 5
            };

            var otherClaims = new List<ItemClaim>
            {
                new ItemClaim
                {
                    Id = 6,
                    ItemId = 1,
                    UserId = 20,
                    Quantity = 2,
                    IsActive = true
                },
                claim
            };

            _claimsMock
                .Setup(x => x.GetByIdAsync(
                    claimId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(claim);

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            _claimsMock
                .Setup(x => x.GetActiveClaimsAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(otherClaims);

            _claimsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(
                claimId, dto, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(3, claim.Quantity);

            _claimsMock.Verify(
                x => x.Update(claim),
                Times.Once);

            _itemsMock.Verify(
                x => x.Update(item),
                Times.Once);

            _claimsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_DifferentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var claim = new ItemClaim
            {
                Id = 5,
                ItemId = 1,
                UserId = 10,
                Quantity = 1,
                IsActive = true
            };

            _claimsMock
                .Setup(x => x.GetByIdAsync(
                    5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(claim);

            var dto = new CreateItemClaimDto
            {
                ItemId = 1,
                Quantity = 2
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.UpdateAsync(5, dto, 20));

            _claimsMock.Verify(
                x => x.Update(It.IsAny<ItemClaim>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelAsync_ValidClaim_CancelsSuccessfully()
        {
            // Arrange
            const int claimId = 5;
            const int userId = 10;

            var claim = new ItemClaim
            {
                Id = claimId,
                ItemId = 1,
                UserId = userId,
                Quantity = 2,
                IsActive = true
            };

            var item = new Item
            {
                Id = 1,
                Quantity = 5
            };

            _claimsMock
                .Setup(x => x.GetByIdAsync(
                    claimId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(claim);

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            _claimsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CancelAsync(claimId, userId);

            // Assert
            Assert.True(result);
            Assert.False(claim.IsActive);

            _claimsMock.Verify(
                x => x.Update(claim),
                Times.Once);

            _itemsMock.Verify(
                x => x.Update(item),
                Times.Once);

            _claimsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelAsync_DifferentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var claim = new ItemClaim
            {
                Id = 5,
                ItemId = 1,
                UserId = 10,
                Quantity = 2,
                IsActive = true
            };

            _claimsMock
                .Setup(x => x.GetByIdAsync(
                    5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(claim);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CancelAsync(5, 20));

            _claimsMock.Verify(
                x => x.Update(It.IsAny<ItemClaim>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelAsync_NonExistingClaim_ReturnsFalse()
        {
            // Arrange
            _claimsMock
                .Setup(x => x.GetByIdAsync(
                    999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ItemClaim?)null);

            // Act
            var result = await _service.CancelAsync(999, 10);

            // Assert
            Assert.False(result);

            _claimsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
