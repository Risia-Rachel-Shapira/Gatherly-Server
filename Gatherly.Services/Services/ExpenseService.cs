
using AutoMapper;
using Gatherly.Core.DTOs.Expenses;
using Gatherly.Core.Models;
using Gatherly.Core.Repositories;
using Gatherly.Core.Services;

namespace Gatherly.Services.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repository;
        private readonly IMapper _mapper;
        private readonly IEventAccessService _eventAccess;

        public ExpenseService(
            IExpenseRepository repository,
            IMapper mapper,
            IEventAccessService eventAccess)
        {
            _repository = repository;
            _mapper = mapper;
            _eventAccess = eventAccess;
        }

        public async Task<ExpenseResponseDto> CreateAsync(
            CreateExpenseDto dto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            await _eventAccess.EnsureAccessAsync(
                dto.EventId, userId, cancellationToken);

            var expense = _mapper.Map<Expense>(dto);
            expense.PaidByUserId = userId;

            await _repository.AddAsync(expense, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ExpenseResponseDto>(expense);
        }

        public async Task<List<ExpenseResponseDto>> GetByEventAsync(
            int eventId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await _eventAccess.EnsureAccessAsync(
                eventId, userId, cancellationToken);

            var expenses = await _repository.GetByEventAsync(
                eventId, cancellationToken);

            return _mapper.Map<List<ExpenseResponseDto>>(expenses);
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var expense = await _repository.GetByIdAsync(
                id, cancellationToken);

            if (expense == null)
                return false;

            await _eventAccess.EnsureAccessAsync(
                expense.EventId, userId, cancellationToken);

            if (expense.PaidByUserId != userId)
                throw new UnauthorizedAccessException();

            _repository.Delete(expense);

            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
