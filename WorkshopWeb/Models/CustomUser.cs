using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkshopWeb.Data.ENum;

namespace WorkshopWeb.Models
{
    public class CustomUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Surname { get; set; }

        public Role Role { get; set; }

        public int Wage { get; set; }

        public ICollection<TimeSlot>? TimeSlots { get; set; }

        public ICollection<UserTicket>? UserTickets { get; set; }
    }
}
