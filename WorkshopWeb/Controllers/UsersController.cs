using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopWeb.Data;
using WorkshopWeb.Models;

namespace WorkshopWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly WorkshopDbContext _context;

        public UsersController(UserManager<CustomUser> userManager, WorkshopDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if the user is an admin
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                TempData["Error"] = "Admin users cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Message"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Error deleting user.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.UserTickets)
                    .ThenInclude(ut => ut.Ticket)
                        .ThenInclude(t => t.TimeSlots)
                .Include(u => u.UserTickets)
                    .ThenInclude(ut => ut.Ticket)
                        .ThenInclude(t => t.Parts)
                .Include(u => u.UserTickets)
                    .ThenInclude(ut => ut.Ticket)
                        .ThenInclude(t => t.Estimate)
                .AsNoTracking() // Good for read-only scenarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }



    }
}
