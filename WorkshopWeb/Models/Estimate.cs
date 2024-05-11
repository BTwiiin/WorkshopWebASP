using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkshopWeb.Models
{
    public class Estimate
    {
        [Key]
        public int EstimateId { get; set; }

        public string Description { get; set; }

        public decimal ExpectedCostOfService { get; set; }

        public bool IsAcceptedByClient { get; set; }

        [ForeignKey("Ticket")]
        [Required]
        public int TicketId { get; set; }
    }
}
