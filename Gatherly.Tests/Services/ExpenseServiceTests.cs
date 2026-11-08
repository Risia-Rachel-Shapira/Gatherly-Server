
using AutoMapper;
using Gatherly.Core.DTOs.Expenses;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;
using Gatherly.Services.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Gatherly.Tests.Services
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IExpenseRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ExpenseService _service;
        private readonly Mock<IEventAccessService> _eventAccessMock;

        public ExpenseServiceTests()
        {
            _repositoryMock = new Mock<IExpenseRepository>();
            _mapperMock = new Mock<IMapper>();
            _eventAccessMock = new Mock<IEventAccessService>();

            // Allow access in these unit tests.
            _eventAccessMock
                .Setup(x => x.EnsureAccessAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Event());

            _service = new ExpenseService(
                _repositoryMock.Object,
                _mapperMock.Object,
                _eventAccessMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidExpense_CreatesExpenseSuccessfully()
        {
            // Arrange
            var dto = new CreateExpenseDto
            {
                EventId = 1,
                ItemId = 2,
                Amount = 150,
                Description = "Food"
            };

            const int userId = 10;

            var expense = new Expense
            {
                Id = 1,
                EventId = dto.EventId,
                ItemId = dto.ItemId,
                Amount = dto.Amount,
                Description = dto.Description
            };

            var response = new ExpenseResponseDto
            {
                Id = 1,
                EventId = 1,
                ItemId = 2,
                PaidByUserId = userId,
                Amount = 150,
                Description = "Food"
            };

            _mapperMock
                .Setup(m => m.Map<Expense>(dto))
                .Returns(expense);

            _mapperMock
                .Setup(m => m.Map<ExpenseResponseDto>(expense))
                .Returns(response);

            _repositoryMock
                .Setup(r => r.AddAsync(
                    expense,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(dto, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(userId, expense.PaidByUserId);
            Assert.Equal(150, result.Amount);

            _repositoryMock.Verify(
                r => r.AddAsync(
                    expense,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _repositoryMock.Verify(
                r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidAmount_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateExpenseDto
            {
                EventId = 1,
                Amount = 0
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateAsync(dto, 10));

            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.IsAny<Expense>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _repositoryMock.Verify(
                r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetByEventAsync_ExistingExpenses_ReturnsExpenses()
        {
            // Arrange
            const int eventId = 1;

            var expenses = new List<Expense>
            {
                new Expense
                {
                    Id = 1,
                    EventId = eventId,
                    Amount = 100
                },
                new Expense
                {
                    Id = 2,
                    EventId = eventId,
                    Amount = 200
                }
            };

            var expected = new List<ExpenseResponseDto>
            {
                new ExpenseResponseDto { Id = 1, Amount = 100 },
                new ExpenseResponseDto { Id = 2, Amount = 200 }
            };

            _repositoryMock
                .Setup(r => r.GetByEventAsync(
                    eventId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expenses);

            _mapperMock
                .Setup(m => m.Map<List<ExpenseResponseDto>>(expenses))
                .Returns(expected);

            // Act
            var result = await _service.GetByEventAsync(eventId, 10);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(100, result[0].Amount);
            Assert.Equal(200, result[1].Amount);
        }

        [Fact]
        public async Task DeleteAsync_ExpenseOwner_DeletesSuccessfully()
        {
            // Arrange
            const int expenseId = 1;
            const int userId = 10;

            var expense = new Expense
            {
                Id = expenseId,
                PaidByUserId = userId
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    expenseId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expense);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(expenseId, userId);

            // Assert
            Assert.True(result);

            _repositoryMock.Verify(
                r => r.Delete(expense),
                Times.Once);

            _repositoryMock.Verify(
                r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DifferentUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var expense = new Expense
            {
                Id = 1,
                PaidByUserId = 10
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    1,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expense);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DeleteAsync(1, 20));

            _repositoryMock.Verify(
                r => r.Delete(It.IsAny<Expense>()),
                Times.Never);

            _repositoryMock.Verify(
                r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingExpense_ReturnsFalse()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    999,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Expense?)null);

            // Act
            var result = await _service.DeleteAsync(999, 10);

            // Assert
            Assert.False(result);

            _repositoryMock.Verify(
                r => r.Delete(It.IsAny<Expense>()),
                Times.Never);

            _repositoryMock.Verify(
                r => r.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
