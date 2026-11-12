using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Gatherly.Core.DTOs.Users
{
    public class UserRegisterDto
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "יש להזין סיסמה")]
        [StringLength(64, MinimumLength = 12,
         ErrorMessage = "הסיסמה חייבת להכיל בין 12 ל-64 תווים")]
        public string Password { get; set; }
    }
}
