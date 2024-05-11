using System.Diagnostics;
using System.IO;
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            var parts = _context.Parts.Where(p => p.TicketId == ticketId);
            _context.Parts.RemoveRange(parts);
            var estimate = _context.Estimates.FirstOrDefault(e => e.TicketId == ticketId);
            if (estimate != null)
            {
                _context.Estimates.Remove(estimate);
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
            .Include(t => t.TimeSlots)
            .Include(t => t.Parts)
            .Include(t => t.Estimate) 
            .FirstOrDefault(t => t.TicketId == ticketId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }


    public IActionResult EditTicket(int ticketId)
    {
        var ticket = _context.Tickets
            .Include(t => t.Parts)
            .Include(t => t.Estimate)
            .FirstOrDefault(t => t.TicketId == ticketId);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTicket(Ticket ticket, int OriginalAmountOfSlotsNeeded, List<Part> parts, Estimate estimate)
    {
        if (ModelState.IsValid)
        {
            var existingTicket = _context.Tickets.Include(t => t.Parts).FirstOrDefault(t => t.TicketId == ticket.TicketId);

            if (existingTicket == null)
            {
                return NotFound(); // Or handle the situation where the ticket isn't found
            }

            if (OriginalAmountOfSlotsNeeded != ticket.AmountOfSlotsNeeded)
            {
                TempData["TicketId"] = ticket.TicketId;
                TempData["AmountOfSlotsNeeded"] = ticket.AmountOfSlotsNeeded - OriginalAmountOfSlotsNeeded;
                _context.Entry(existingTicket).CurrentValues.SetValues(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction("SelectTimeSlots", "Calendar");
            }

            _context.Entry(existingTicket).CurrentValues.SetValues(ticket);

            foreach (var part in parts)
            {
                var existingPart = existingTicket.Parts.FirstOrDefault(p => p.PartId == part.PartId);
                if (existingPart != null)
                {
                    _context.Entry(existingPart).CurrentValues.SetValues(part);
                }
                else
                {
                    part.TicketId = existingTicket.TicketId;
                    _context.Parts.Add(part);
                }
            }

            var existingEstimate = _context.Estimates.FirstOrDefault(e => e.TicketId == ticket.TicketId);
            if (existingEstimate != null)
            {
                _context.Estimates.Remove(existingEstimate);
            }
            estimate.TicketId = existingTicket.TicketId;
            _context.Estimates.Add(estimate);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TicketDetails), new { ticketId = ticket.TicketId });
        }
        return View(ticket);
    }

    [HttpGet]
    public async Task<IActionResult> DeletePart(int partId)
    {
        var part = await _context.Parts.FindAsync(partId);
        if (part != null)
        {
            int ticketId = part.TicketId;
            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            return RedirectToAction("TicketDetails", "Home", new { ticketId = part.TicketId });
        }
        return NotFound();
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
