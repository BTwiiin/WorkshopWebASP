using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkshopWeb.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [MaxLength(20)]
        public string Model { get; set; }

        [MaxLength(20)]
        public string Brand {  get; set; }

        [MaxLength(10)]
        public string RegId { get; set; }

        public string Description { get; set; }

        public ICollection<TimeSlot>? TimeSlots { get; set; }

        public ICollection<UserTicket>? UserTickets { get; set; }

    }
}
