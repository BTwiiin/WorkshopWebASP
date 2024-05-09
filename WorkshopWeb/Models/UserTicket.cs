using System.ComponentModel.DataAnnotations;

namespace WorkshopWeb.Models
{
    public class UserTicket
    {
        [Key]
        public int UserTicketId { get; set; }

        public string CustomUserId { get; set; }

        public CustomUser CustomUser { get; set; }

        public int TicketId { get; set; }

        public Ticket Ticket { get; set; }

    }
}
