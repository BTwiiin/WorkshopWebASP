using System.Collections.Generic;
using WorkshopWeb.Models;


namespace WorkshopWeb.ViewModels
{
    public class SlotSelectionViewModel
    {
        public List<TimeSlot> TimeSlots { get; set; }  // List of available time slots for selection

        // Constructor to initialize the list
        public SlotSelectionViewModel()
        {
            TimeSlots = new List<TimeSlot>();
        }
    }
}
