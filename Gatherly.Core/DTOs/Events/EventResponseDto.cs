using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherly.Core.DTOs.Events
{
    public class EventResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public string? Location { get; set; }

        public decimal? Budget { get; set; }

        public int OrganizerId { get; set; }

        public string OrganizerName { get; set; } = string.Empty;

        public int ManagementType { get; set; }
    }
}
