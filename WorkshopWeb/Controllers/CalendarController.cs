using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkshopWeb.Data;
using WorkshopWeb.Models;
using WorkshopWeb.ViewModels;

public class CalendarController : Controller
{
    private readonly WorkshopDbContext _context;
    private readonly UserManager<CustomUser> _userManager;

    public CalendarController(WorkshopDbContext context, UserManager<CustomUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<IActionResult> Index(DateTime? date)
    {
        if (!date.HasValue)
        {
            date = DateTime.Today;
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge(); // Ensure the user is logged in
        }

        var slotsToday = _context.TimeSlots
                             .Where(t => t.EmployeeId == user.Id && t.StartTime.Date == date.Value.Date)
                             .OrderBy(t => t.StartTime)
                             .ToList();

        var nextDay = date.Value.AddDays(1);

        ViewBag.PreviousDate = date.Value.AddDays(-1);
        ViewBag.NextDate = nextDay;
        ViewBag.CurrentDate = date.Value;

        return View(slotsToday);
    }

    public async Task<IActionResult> Slots(DateTime? date, int? year, int? month)
    {
        if (!year.HasValue || !month.HasValue)
        {
            date = DateTime.Today;
        }
        else
        {
            date = new DateTime(year.Value, month.Value, 1); // Use the first day of the specified month
        }

        DateTime firstDayOfMonth = new DateTime(date.Value.Year, date.Value.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(date.Value.Year, date.Value.Month);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge(); // Ensure the user is logged in
        }

        // Get all slots for the month for the user
        var monthSlots = await _context.TimeSlots
                                 .Where(t => t.EmployeeId == user.Id &&
                                             t.StartTime.Month == date.Value.Month &&
                                             t.StartTime.Year == date.Value.Year)
                                 .ToListAsync();

        // Determine which days have slots
        HashSet<int> daysWithSlots = new HashSet<int>();
        foreach (var slot in monthSlots)
        {
            daysWithSlots.Add(slot.StartTime.Day);
        }

        if (!year.HasValue || !month.HasValue)
        {
            year = DateTime.Today.Year;
            month = DateTime.Today.Month;
        }

        var daysFullyBooked = new HashSet<int>();
        var daysWithAvailableSlots = new HashSet<int>();

        foreach (var slot in monthSlots.GroupBy(t => t.StartTime.Date))
        {
            if (slot.All(s => s.IsBooked))
            {
                daysFullyBooked.Add(slot.Key.Day);
            }
            else
            {
                daysWithAvailableSlots.Add(slot.Key.Day);
            }
        }

        ViewBag.DaysFullyBooked = daysFullyBooked;
        ViewBag.DaysWithAvailableSlots = daysWithAvailableSlots;

        DateTime currentDate = new DateTime(year.Value, month.Value, 1);
        DateTime previousMonthDate = currentDate.AddMonths(-1);
        DateTime nextMonthDate = currentDate.AddMonths(1);

        ViewBag.DaysInMonth = daysInMonth;
        ViewBag.FirstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
        ViewBag.CurrentMonth = firstDayOfMonth.ToString("MMMM yyyy");
        ViewBag.Today = DateTime.Today;
        ViewBag.Year = currentDate.Year;
        ViewBag.Month = currentDate.Month;
        ViewBag.DaysWithSlots = daysWithSlots;
        ViewBag.PreviousMonthYear = previousMonthDate.Year;
        ViewBag.PreviousMonth = previousMonthDate.Month;
        ViewBag.NextMonthYear = nextMonthDate.Year;
        ViewBag.NextMonth = nextMonthDate.Month;

        return View();
    }

    private void GenerateTimeSlotsForDay(DateTime date, string userId)
    {
        var startOfDay = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        bool slotsExist = _context.TimeSlots.Any(t => t.StartTime >= startOfDay && t.StartTime < startOfDay.AddDays(1));

        if (!slotsExist)
        {
            for (int hour = 8; hour < 18; hour++)
            {
                if (hour == 12) continue;
                _context.TimeSlots.Add(new TimeSlot
                {
                    StartTime = startOfDay.AddHours(hour),
                    EmployeeId = userId,
                    IsBooked = false
                });
                _context.SaveChanges();
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> DisableSlot(int slotId, DateTime date)
    {
        var slot = await _context.TimeSlots.FirstOrDefaultAsync(t => t.TimeSlotId == slotId);
        if (slot != null && !slot.IsBooked)
        {
            slot.IsBooked = true;
            _context.Update(slot);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { date = date });
        }
        else
        {
            // Handle the case where the slot is already booked or an error occurs
            return RedirectToAction("Index", new { date = date, error = "Slot already disabled or unavailable" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> EnableSlot(int slotId, DateTime date)
    {
        var slot = await _context.TimeSlots.FirstOrDefaultAsync(t => t.TimeSlotId == slotId);
        if (slot != null && slot.IsBooked)
        {
            slot.IsBooked = false;
            _context.Update(slot);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { date = date });
        }
        else
        {
            // Handle the case where the slot is already booked or an error occurs
            return RedirectToAction("Index", new { date = date, error = "Slot already enabled or unavailable" });
        }
    }

    public IActionResult SelectTimeSlots()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var model = new SlotSelectionViewModel
        {
            TimeSlots = _context.TimeSlots.Where(t => !t.IsBooked && t.EmployeeId == userId).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult SelectTimeSlots(int[] selectedSlots)
    {
        int ticketId = 0;

        if (TempData["TicketId"] is int)
        {
            ticketId = (int)TempData["TicketId"];
        }

        if (TempData["AmountOfSlotsNeeded"] is int)
        {
            var amountOfSlotsNeeded = (int)TempData["AmountOfSlotsNeeded"];
            if (selectedSlots.Length != amountOfSlotsNeeded)
            {
                return RedirectToAction("SelectTimeSlots", new { error = "Please select the correct number of slots" });
            }
        }

        if (!_context.Tickets.Any(t => t.TicketId == ticketId))
        {
            throw new InvalidOperationException("Ticket ID does not exist.");
        }

        

        foreach (var slotId in selectedSlots)
        {
            var slot = _context.TimeSlots.FirstOrDefault(t => t.TimeSlotId == slotId);
            if (slot != null && !slot.IsBooked)
            {
                slot.IsBooked = true;
                slot.TicketId = ticketId; // Link the slot to the ticket
                _context.Update(slot);
            }
        }
        _context.SaveChanges();

        return RedirectToAction("Index", "Home");
    }
}