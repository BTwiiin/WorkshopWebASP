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
            return RedirectToAction("SelectTimeSlots", "Calendar");
        }

        var users = _context.Users.ToList();

        var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();
        ViewBag.Users = new SelectList(users, "Id", "Name");
        return View(ticket);
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
