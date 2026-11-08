using Gatherly.Core.DTOs.Events;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gatherly.API.Controllers.Events
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
                return BadRequest();

            var result = await _eventService.GetByIdAsync(id, GetUserId(), cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(
                    "Page number must be at least 1 and page size must be between 1 and 100.");

            var result = await _eventService.GetAllAsync(GetUserId(), pageNumber, pageSize, cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateEventDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _eventService.CreateAsync(
                dto, GetUserId(), cancellationToken);

            return CreatedAtAction(
                nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateEventDto dto,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
                return BadRequest();

            var updated = await _eventService.UpdateAsync(
                id, dto, GetUserId(), cancellationToken);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
                return BadRequest();

            var deleted = await _eventService.DeleteAsync(
                id, GetUserId(), cancellationToken);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}