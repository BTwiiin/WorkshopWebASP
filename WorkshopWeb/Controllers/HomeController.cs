using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopWeb.Data;
using WorkshopWeb.Models;

namespace WorkshopWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly WorkshopDbContext _context;

    public HomeController(ILogger<HomeController> logger, WorkshopDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var tickets = await _context.Tickets
                                    .Include(t => t.UserTickets)
                                    .ThenInclude(ut => ut.CustomUser)
                                    .ToListAsync();
        return View(tickets);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CreateTicket()
    {
        var users = _context.Users.ToList();
        ViewBag.Users = new SelectList(users, "Id", "Name");
        return View(new Ticket());
    }

    [HttpPost]
    public IActionResult CreateTicket(Ticket ticket, string[] userIds)
    {
        if (ModelState.IsValid)
        {
            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current user's ID
            if (userId != null)
            {
                var userTicket = new UserTicket
                {
                    TicketId = ticket.TicketId,
                    CustomUserId = userId
                };
                _context.UserTickets.Add(userTicket);
            }

            _context.SaveChanges();
            TempData["TicketId"] = ticket.TicketId;
            TempData["AmountOfSlotsNeeded"] = ticket.AmountOfSlotsNeeded;
            return RedirectToAction("SelectTimeSlots", "Calendar");
        }

        var users = _context.Users.ToList();

/*        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();*/
        ViewBag.Users = new SelectList(users, "Id", "Name");
        return View(ticket);
    }

    [HttpPost]
    public IActionResult DeleteTicket(int ticketId)
    {
        var ticket = _context.Tickets.FirstOrDefault(t => t.TicketId == ticketId);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            var slots = _context.TimeSlots.Where(t => t.TicketId == ticketId);
            foreach (var slot in slots)
            {
                slot.TicketId = null;
                slot.IsBooked = false;
                _context.Update(slot);
            }
            _context.RemoveRange(_context.UserTickets.Where(ut => ut.TicketId == ticketId));
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    public IActionResult TicketDetails(int ticketId)
    {
        var ticket = _context.Tickets
            .Include(t => t.UserTickets)
            .ThenInclude(ut => ut.CustomUser)
            .FirstOrDefault(t => t.TicketId == ticketId);
        return View(ticket);
    }

    public IActionResult EditTicket(int ticketId)
    {
        var ticket = _context.Tickets.Find(ticketId);
        if (ticket == null)
        {
            return NotFound();
        }
        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditTicket(Ticket ticket, int OriginalAmountOfSlotsNeeded)
    {
        if (ModelState.IsValid)
        {
            var originalTicket = _context.Tickets.AsNoTracking().FirstOrDefault(t => t.TicketId == ticket.TicketId);
            if (originalTicket == null)
            {
                return NotFound();
            }

            if (OriginalAmountOfSlotsNeeded != ticket.AmountOfSlotsNeeded)
            {
                TempData["TicketId"] = ticket.TicketId;
                TempData["AmountOfSlotsNeeded"] = ticket.AmountOfSlotsNeeded - OriginalAmountOfSlotsNeeded;
                _context.Update(ticket);
                _context.SaveChanges();
                return RedirectToAction("SelectTimeSlots", "Calendar");
            }
            _context.Update(ticket);
            _context.SaveChanges();
            return RedirectToAction(nameof(TicketDetails), new { ticketId = ticket.TicketId });
        }
        return View(ticket);
    }




    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
