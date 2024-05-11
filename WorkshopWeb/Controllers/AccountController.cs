using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopWeb.Data;
using WorkshopWeb.Models;
using WorkshopWeb.Models.AccountModels;

namespace WorkshopWeb.Controllers
{
    public class AccountController(SignInManager<CustomUser> signInManager, UserManager<CustomUser> userManager, WorkshopDbContext _context) : Controller
    {
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Username!, model.Password!, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var user = await userManager.GetUserAsync(User);
                    EnsureFutureSlots(DateTime.UtcNow, user.Id);
                    DeletePastTimeSlots();
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt");
            }
            return View(model);
        }

        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                CustomUser user = new()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Wage = model.Wage,
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    GenerateMonthOfTimeSlots(user.Id, DateTime.UtcNow);

                    //await signInManager.SignInAsync(user, false);

                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(HomeController.Index), nameof(HomeController));
        }

        private void GenerateMonthOfTimeSlots(string userId, DateTime startDate)
        {
            var timeSlots = new List<TimeSlot>();

            for (int day = 0; day < 30; day++) // Loop for a week
            {
                DateTime date = startDate.AddDays(day);
                for (int hour = 8; hour < 18; hour++) // Daily slots from 8 AM to 5 PM, skipping 12 PM
                {
                    if (hour == 12) continue; // skip lunch hour

                    timeSlots.Add(new TimeSlot
                    {
                        EmployeeId = userId,
                        StartTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0),
                        IsBooked = false
                    });
                }
            }

            _context.TimeSlots.AddRange(timeSlots);
            _context.SaveChanges();
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

        private void EnsureFutureSlots(DateTime today, string userId)
        {
            var thirtyDaysAhead = today.AddDays(30);
            var slotsExist = _context.TimeSlots
                                     .Any(t => t.StartTime.Date > today && t.StartTime.Date <= thirtyDaysAhead && t.EmployeeId == userId);

            if (!slotsExist)
            {
                for (DateTime day = today.AddDays(1); day <= thirtyDaysAhead; day = day.AddDays(1))
                {
                    if (!_context.TimeSlots.Any(t => t.StartTime.Date == day.Date && t.EmployeeId == userId))
                    {
                        GenerateTimeSlotsForDay(day, userId);
                    }
                }
            }
        }

        private void DeletePastTimeSlots()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var pastSlots = _context.TimeSlots.Where(t => t.StartTime.Date <= yesterday).ToList();

            if (pastSlots.Any())
            {
                _context.TimeSlots.RemoveRange(pastSlots);
                _context.SaveChanges();
            }
        }
    }
}
