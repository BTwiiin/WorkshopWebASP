using System.ComponentModel.DataAnnotations;

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

        public int? TicketId { get; set; }
    }
}
