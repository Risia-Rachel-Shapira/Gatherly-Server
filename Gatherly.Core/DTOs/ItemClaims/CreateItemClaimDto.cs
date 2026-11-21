using System.ComponentModel.DataAnnotations;

namespace Gatherly.Core.DTOs.ItemClaims
{
    public class CreateItemClaimDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "מזהה פריט לא תקין")]
        public int ItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "הכמות חייבת להיות גדולה מאפס")]
        public int Quantity { get; set; }
    }
}
