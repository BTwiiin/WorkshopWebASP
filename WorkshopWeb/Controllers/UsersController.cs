using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopWeb.Data;
using WorkshopWeb.Models;

namespace WorkshopWeb.Controllers
{
    public class UsersController : Controller
    {
        private readonly WorkshopDbContext _context;

        public UsersController(WorkshopDbContext context)
        {
            _context = context;
        }

        // GET: Show the Add User Form
        [HttpGet]
        public IActionResult AddUser()
        {
            return View(new CustomUser()); // Assumes you have a View named "AddUser"
        }

        // POST: Process the Add User Form
        [HttpPost]
        public async Task<IActionResult> AddUser(CustomUser user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home"); // Redirect to a confirmation page or another appropriate action
            }
            return View(user); // Return the view with the current user data if validation fails
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
    }
}
