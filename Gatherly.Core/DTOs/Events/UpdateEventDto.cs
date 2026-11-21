using System.ComponentModel.DataAnnotations;

namespace Gatherly.Core.DTOs.Events
{
    public class UpdateEventDto
    {
        [Required(ErrorMessage = "יש להזין שם לאירוע")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "יש להזין תאריך")]
        public DateTime Date { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
            ErrorMessage = "התקציב חייב להיות גדול מאפס")]
        public decimal? Budget { get; set; }
    }
}
