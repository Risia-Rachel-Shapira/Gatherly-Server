
using AutoMapper;
using Gatherly.Core.DTOs.Items;
using Gatherly.Core.Exceptions;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Services.Services;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class ItemServiceTests
    {
        private readonly Mock<IItemRepository> _itemsMock;
        private readonly Mock<IItemClaimRepository> _claimsMock;
        private readonly Mock<IExpenseRepository> _expensesMock;
        private readonly Mock<IEventRepository> _eventsMock;
        private readonly Mock<IEventMemberRepository> _membersMock;
        private readonly Mock<IUserRepository> _usersMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly ItemService _service;

        public ItemServiceTests()
        {
            _itemsMock = new Mock<IItemRepository>();
            _claimsMock = new Mock<IItemClaimRepository>();
            _expensesMock = new Mock<IExpenseRepository>();
            _eventsMock = new Mock<IEventRepository>();
            _membersMock = new Mock<IEventMemberRepository>();
            _usersMock = new Mock<IUserRepository>();
            _mapperMock = new Mock<IMapper>();

            _service = new ItemService(
                _itemsMock.Object,
                _claimsMock.Object,
                _expensesMock.Object,
                _eventsMock.Object,
                _membersMock.Object,
                _usersMock.Object,
                _mapperMock.Object);
        }

        // Common access setup
        private void SetupAccess(
            int eventId,
            int organizerId,
            int userId,
            bool isActiveMember = true)
        {
            _eventsMock
                .Setup(x => x.GetByIdAsync(
                    eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event
                {
                    Id = eventId,
                    OrganizerId = organizerId
                });

            _usersMock
                .Setup(x => x.GetByIdAsync(
                    userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Id = userId
                });

            if (organizerId != userId)
            {
                _membersMock
                    .Setup(x => x.GetByEventAndUserAsync(
                        eventId,
                        userId,
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        isActiveMember
                            ? new EventMember
                            {
                                EventId = eventId,
                                UserId = userId,
                                IsActive = true,
                                Role = "Participant"
                            }
                            : null);
            }
        }

        [Fact]
        public async Task CreateAsync_ActiveParticipant_CreatesItem()
        {
            // Arrange
            const int eventId = 5;
            const int userId = 20;

            SetupAccess(eventId, organizerId: 10, userId);

            var dto = new CreateItemDto
            {
                EventId = eventId,
                Name = "Drinks",
                Quantity = 3,
                EstimatedCost = 50
            };

            var item = new Item
            {
                EventId = eventId,
                Name = "Drinks",
                Quantity = 3
            };

            var response = new ItemResponseDto
            {
                EventId = eventId,
                Name = "Drinks",
                Quantity = 3,
                ClaimedQuantity = 0,
                RemainingQuantity = 3
            };

            _mapperMock
                .Setup(x => x.Map<Item>(dto))
                .Returns(item);

            _mapperMock
                .Setup(x => x.Map<ItemResponseDto>(item))
                .Returns(response);

            _itemsMock
                .Setup(x => x.AddAsync(
                    It.IsAny<Item>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _itemsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Quantity);
            Assert.Equal(3, result.RemainingQuantity);
            Assert.Equal(userId, item.AddedByUserId);

            _itemsMock.Verify(
                x => x.AddAsync(
                    It.Is<Item>(i =>
                        i.EventId == eventId &&
                        i.AddedByUserId == userId),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ParticipantCanUpdateOwnItem()
        {
            // Arrange
            const int itemId = 1;
            const int userId = 20;
            const int eventId = 5;

            SetupAccess(eventId, organizerId: 10, userId);

            var item = new Item
            {
                Id = itemId,
                EventId = eventId,
                AddedByUserId = userId,
                Quantity = 5
            };

            var dto = new UpdateItemDto
            {
                Name = "Updated drinks",
                Quantity = 4
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            _claimsMock
                .Setup(x => x.GetActiveClaimsAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemClaim>());

            _itemsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(
                itemId, dto, userId);

            // Assert
            Assert.True(result);

            _itemsMock.Verify(
                x => x.Update(item),
                Times.Once);

            _itemsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ParticipantCannotUpdateAnotherUsersItem()
        {
            // Arrange
            const int itemId = 1;
            const int userId = 20;

            SetupAccess(eventId: 5, organizerId: 10, userId);

            var item = new Item
            {
                Id = itemId,
                EventId = 5,
                AddedByUserId = 30,
                Quantity = 5
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            var dto = new UpdateItemDto
            {
                Name = "Unauthorized update",
                Quantity = 4
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.UpdateAsync(itemId, dto, userId));

            _itemsMock.Verify(
                x => x.Update(It.IsAny<Item>()),
                Times.Never);

            _itemsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ParticipantCannotDeleteAnotherUsersItem()
        {
            // Arrange
            const int itemId = 1;
            const int userId = 20;

            SetupAccess(eventId: 5, organizerId: 10, userId);

            var item = new Item
            {
                Id = itemId,
                EventId = 5,
                AddedByUserId = 30,
                Quantity = 5
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DeleteAsync(itemId, userId));

            _itemsMock.Verify(
                x => x.Delete(It.IsAny<Item>()),
                Times.Never);

            _itemsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_OrganizerCanUpdateAnotherUsersItem()
        {
            // Arrange
            const int itemId = 1;
            const int organizerId = 10;

            SetupAccess(
                eventId: 5,
                organizerId: organizerId,
                userId: organizerId);

            var item = new Item
            {
                Id = itemId,
                EventId = 5,
                AddedByUserId = 20,
                Quantity = 5
            };

            var dto = new UpdateItemDto
            {
                Name = "Organizer update",
                Quantity = 4
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            _claimsMock
                .Setup(x => x.GetActiveClaimsAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemClaim>());

            _itemsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateAsync(
                itemId, dto, organizerId);

            // Assert
            Assert.True(result);

            _itemsMock.Verify(
                x => x.Update(item),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_OrganizerCanDeleteAnotherUsersItem()
        {
            // Arrange
            const int itemId = 1;
            const int organizerId = 10;

            SetupAccess(
                eventId: 5,
                organizerId: organizerId,
                userId: organizerId);

            var item = new Item
            {
                Id = itemId,
                EventId = 5,
                AddedByUserId = 20,
                Quantity = 5
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            SetupDeleteDependencies(
                itemId, new List<ItemClaim>(), hasExpenses: false);

            _itemsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(
                itemId, organizerId);

            // Assert
            Assert.True(result);

            _itemsMock.Verify(
                x => x.Delete(item),
                Times.Once);

            _itemsMock.Verify(
                x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByEventAsync_UsesSingleGroupedClaimsQuery()
        {
            // Arrange
            const int eventId = 5;
            const int userId = 20;

            SetupAccess(eventId, organizerId: 10, userId);

            var items = new List<Item>
            {
                new Item { Id = 1, EventId = eventId, Quantity = 5 },
                new Item { Id = 2, EventId = eventId, Quantity = 3 },
                new Item { Id = 3, EventId = eventId, Quantity = 2 }
            };

            _itemsMock
                .Setup(x => x.GetByEventAsync(
                    eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            // Item 3 has no active claims, so it is missing here.
            _claimsMock
                .Setup(x => x.GetClaimedQuantitiesByEventAsync(
                    eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<int, int>
                {
                    [1] = 2,
                    [2] = 3
                });

            _mapperMock
                .Setup(x => x.Map<ItemResponseDto>(It.IsAny<Item>()))
                .Returns((object src) =>
                {
                    var i = (Item)src;
                    return new ItemResponseDto
                    {
                        Id = i.Id,
                        EventId = i.EventId,
                        Quantity = i.Quantity
                    };
                });

            // Act
            var result = await _service.GetByEventAsync(eventId, userId);

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Equal(2, result[0].ClaimedQuantity);
            Assert.Equal(3, result[0].RemainingQuantity);

            Assert.Equal(3, result[1].ClaimedQuantity);
            Assert.Equal(0, result[1].RemainingQuantity);

            Assert.Equal(0, result[2].ClaimedQuantity);
            Assert.Equal(2, result[2].RemainingQuantity);

            _claimsMock.Verify(
                x => x.GetClaimedQuantitiesByEventAsync(
                    eventId, It.IsAny<CancellationToken>()),
                Times.Once);

            // No per-item claims query (N+1).
            _claimsMock.Verify(
                x => x.GetActiveClaimsAsync(
                    It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetByEventAsync_InactiveMember_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            const int eventId = 5;
            const int userId = 20;

            SetupAccess(
                eventId,
                organizerId: 10,
                userId,
                isActiveMember: false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.GetByEventAsync(eventId, userId));

            _itemsMock.Verify(
                x => x.GetByEventAsync(
                    eventId, It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private Item SetupItemForDelete(
            int itemId,
            int eventId,
            int organizerId)
        {
            SetupAccess(
                eventId,
                organizerId: organizerId,
                userId: organizerId);

            var item = new Item
            {
                Id = itemId,
                EventId = eventId,
                AddedByUserId = organizerId,
                Quantity = 5
            };

            _itemsMock
                .Setup(x => x.GetByIdAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            return item;
        }

        private void SetupDeleteDependencies(
            int itemId,
            List<ItemClaim> claims,
            bool hasExpenses)
        {
            _claimsMock
                .Setup(x => x.GetByItemAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(claims);

            _expensesMock
                .Setup(x => x.AnyByItemAsync(
                    itemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hasExpenses);
        }

        [Fact]
        public async Task DeleteAsync_ItemWithActiveClaims_ThrowsConflictException()
        {
            // Arrange
            const int itemId = 1;
            SetupItemForDelete(itemId, eventId: 5, organizerId: 10);

            SetupDeleteDependencies(
                itemId,
                new List<ItemClaim>
                {
                    new ItemClaim { Id = 1, ItemId = itemId, Quantity = 2, IsActive = true },
                    new ItemClaim { Id = 2, ItemId = itemId, Quantity = 1, IsActive = false }
                },
                hasExpenses: false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(
                () => _service.DeleteAsync(itemId, 10));

            Assert.Equal(
                "אי אפשר למחוק פריט שמשתתפים כבר תפסו. בטלו קודם את התפיסות.",
                ex.Message);

            _claimsMock.Verify(
                x => x.Delete(It.IsAny<ItemClaim>()), Times.Never);
            _itemsMock.Verify(
                x => x.Delete(It.IsAny<Item>()), Times.Never);
            _itemsMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ItemWithLinkedExpenses_ThrowsConflictException()
        {
            // Arrange
            const int itemId = 1;
            SetupItemForDelete(itemId, eventId: 5, organizerId: 10);

            SetupDeleteDependencies(
                itemId, new List<ItemClaim>(), hasExpenses: true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ConflictException>(
                () => _service.DeleteAsync(itemId, 10));

            Assert.Equal(
                "אי אפשר למחוק פריט שיש לו הוצאות מקושרות. מחקו קודם את ההוצאות.",
                ex.Message);

            _itemsMock.Verify(
                x => x.Delete(It.IsAny<Item>()), Times.Never);
            _itemsMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ItemWithOnlyInactiveClaims_DeletesClaimsAndItemInOneSave()
        {
            // Arrange
            const int itemId = 1;
            var item = SetupItemForDelete(
                itemId, eventId: 5, organizerId: 10);

            var inactive1 = new ItemClaim { Id = 1, ItemId = itemId, Quantity = 2, IsActive = false };
            var inactive2 = new ItemClaim { Id = 2, ItemId = itemId, Quantity = 1, IsActive = false };

            SetupDeleteDependencies(
                itemId,
                new List<ItemClaim> { inactive1, inactive2 },
                hasExpenses: false);

            _itemsMock
                .Setup(x => x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(itemId, 10);

            // Assert
            Assert.True(result);

            _claimsMock.Verify(x => x.Delete(inactive1), Times.Once);
            _claimsMock.Verify(x => x.Delete(inactive2), Times.Once);
            _itemsMock.Verify(x => x.Delete(item), Times.Once);

            // One shared DbContext: a single SaveChanges commits both.
            _itemsMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            _claimsMock.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
