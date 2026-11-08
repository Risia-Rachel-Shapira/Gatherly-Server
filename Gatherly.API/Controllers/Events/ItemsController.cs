using Gatherly.Core.DTOs.Items;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gatherly.API.Controllers.Events
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        private int GetUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id, CancellationToken cancellationToken)
        {
            var item = await _itemService.GetByIdAsync(id, GetUserId(), cancellationToken);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(
            int eventId, CancellationToken cancellationToken)
        {
            var items = await _itemService.GetByEventAsync(eventId, GetUserId(), cancellationToken);

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateItemDto dto,
            CancellationToken cancellationToken)
        {
            var item = await _itemService.CreateAsync(
                dto, GetUserId(), cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateItemDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _itemService.UpdateAsync(
                id, dto, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _itemService.DeleteAsync(
                id, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}