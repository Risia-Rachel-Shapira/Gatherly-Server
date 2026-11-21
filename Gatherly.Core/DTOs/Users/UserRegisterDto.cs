using System.ComponentModel.DataAnnotations;

namespace Gatherly.Core.DTOs.Users
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "יש להזין שם מלא")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין אימייל")]
        [EmailAddress(ErrorMessage = "כתובת האימייל אינה תקינה")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין סיסמה")]
        [StringLength(64, MinimumLength = 12,
         ErrorMessage = "הסיסמה חייבת להכיל בין 12 ל-64 תווים")]
        public string Password { get; set; } = string.Empty;
    }
}
