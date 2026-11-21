
using Gatherly.API.Services;
using Gatherly.Core.Models;
using Gatherly.Core.Services;
using Gatherly.Core.DTOs.Users;
using Gatherly.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Gatherly.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtTokenService _tokenService;

        public AuthController(
            IUserService userService,
            JwtTokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] UserRegisterDto dto,
            CancellationToken cancellationToken)
        {
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Role = UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userService.RegisterAsync(
                user, cancellationToken);

            var token = _tokenService.CreateToken(createdUser);

            // 201 Created: a new user resource was created.
            return StatusCode(StatusCodes.Status201Created, new
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = createdUser.Id,
                    FullName = createdUser.FullName,
                    Email = createdUser.Email
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] UserLoginDto dto,
            CancellationToken cancellationToken)
        {
            var user = await _userService.ValidateCredentialsAsync(
                dto.Email, dto.Password, cancellationToken);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email
                }
            });
        }
    }
}
