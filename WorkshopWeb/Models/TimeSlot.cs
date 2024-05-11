using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkshopWeb.Models
{
    public class TimeSlot
    {
        [Key]
        public int TimeSlotId { get; set; }

        public DateTime StartTime { get; set; }

        public bool IsBooked { get; set; }

        public string? EmployeeId { get; set; }

        public CustomUser? Employee { get; set; }

        [ForeignKey("Ticket")]
        public int? TicketId { get; set; }
    }
}
