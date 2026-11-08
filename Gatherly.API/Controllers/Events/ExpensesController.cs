
using Gatherly.Core.DTOs.Expenses;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gatherly.API.Controllers.Events
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IExpenseShareService _expenseShareService;

        public ExpensesController(
            IExpenseService expenseService,
            IExpenseShareService expenseShareService)
        {
            _expenseService = expenseService;
            _expenseShareService = expenseShareService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(
            int eventId,
            CancellationToken cancellationToken)
        {
            var expenses = await _expenseService.GetByEventAsync(
    eventId, GetUserId(), cancellationToken);

            return Ok(expenses);
        }

        [HttpGet("event/{eventId}/share")]
        public async Task<IActionResult> GetExpenseShare(
            int eventId,
            CancellationToken cancellationToken)
        {
            var result = await _expenseShareService.GetShareAsync(
                eventId, GetUserId(), cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateExpenseDto dto,
            CancellationToken cancellationToken)
        {
            var expense = await _expenseService.CreateAsync(
                dto, GetUserId(), cancellationToken);

            return CreatedAtAction(
                nameof(GetByEvent),
                new { eventId = expense.EventId },
                expense);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _expenseService.DeleteAsync(
                id, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
