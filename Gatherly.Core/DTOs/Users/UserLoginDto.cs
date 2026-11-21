using System.ComponentModel.DataAnnotations;

namespace Gatherly.Core.DTOs.Users
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "יש להזין אימייל")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // No minimum length here: login must accept any existing password.
        [Required(ErrorMessage = "יש להזין סיסמה")]
        [StringLength(64)]
        public string Password { get; set; } = string.Empty;
    }
}
