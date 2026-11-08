using Gatherly.Core.DTOs.ItemClaims;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gatherly.API.Controllers.Events
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItemClaimsController : ControllerBase
    {
        private readonly IItemClaimService _itemClaimService;

        public ItemClaimsController(IItemClaimService itemClaimService)
        {
            _itemClaimService = itemClaimService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetByItem(
    int itemId,
    CancellationToken cancellationToken)
        {
            var claims = await _itemClaimService.GetByItemAsync(
                itemId,
                GetUserId(),
                cancellationToken);

            return Ok(claims);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateItemClaimDto dto,
            CancellationToken cancellationToken)
        {
            var claim = await _itemClaimService.CreateAsync(
                dto, GetUserId(), cancellationToken);

            return Ok(claim);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CreateItemClaimDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _itemClaimService.UpdateAsync(
                id, dto, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(
            int id, CancellationToken cancellationToken)
        {
            var result = await _itemClaimService.CancelAsync(
                id, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}