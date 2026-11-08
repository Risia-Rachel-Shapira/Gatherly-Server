using Gatherly.Core.DTOs.Contributions;
using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gatherly.API.Controllers.Events
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContributionsController : ControllerBase
    {
        private readonly IContributionService _contributionService;

        public ContributionsController(IContributionService contributionService)
        {
            _contributionService = contributionService;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(
            int eventId, CancellationToken cancellationToken)
        {
            var contributions = await _contributionService.GetByEventAsync(
    eventId, GetUserId(), cancellationToken);

            return Ok(contributions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateContributionDto dto,
            CancellationToken cancellationToken)
        {
            var contribution = await _contributionService.CreateAsync(
                dto, GetUserId(), cancellationToken);

            return CreatedAtAction(
                nameof(GetByEvent),
                new { eventId = contribution.EventId },
                contribution);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id, [FromBody] CreateContributionDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _contributionService.UpdateAsync(
                id, dto, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id, CancellationToken cancellationToken)
        {
            var result = await _contributionService.DeleteAsync(
                id, GetUserId(), cancellationToken);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}