using Gatherly.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.API.Controllers.Admin
{
    // Global role from the JWT. Authenticated non-admin users get 403.
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(
                    "Page number must be at least 1 and page size must be between 1 and 100.");

            var result = await _adminService.GetUsersAsync(
                pageNumber, pageSize, cancellationToken);

            return Ok(result);
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(
                    "Page number must be at least 1 and page size must be between 1 and 100.");

            var result = await _adminService.GetEventsAsync(
                pageNumber, pageSize, cancellationToken);

            return Ok(result);
        }

        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(
            [FromRoute] int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
                return BadRequest();

            var deleted = await _adminService.DeleteEventAsync(
                id, cancellationToken);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
