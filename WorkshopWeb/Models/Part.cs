using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkshopWeb.Models
{
    public class Part
    {
        [Key]
        public int PartId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Amount * UnitPrice;

        [ForeignKey("Ticket")]
        [Required]
        public int TicketId { get; set; }
    }
}
