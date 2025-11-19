// Controllers/LoginController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models.ViewModels;
using ST10448895_CMCS_PROG.Helpers;

namespace ST10448895_CMCS_PROG.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Index()
        {
            // Clear any existing session
            HttpContext.Session.Clear();
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

            if (user == null)
            {
                TempData["Error"] = "Invalid username or password.";
                return View(model);
            }

            // Verify password
            if (!PasswordHasher.VerifyPassword(user.PasswordHash, model.Password))
            {
                TempData["Error"] = "Invalid username or password.";
                return View(model);
            }

            // Update last login
            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            // Get role-specific ID and name
            int roleId = 0;
            string userName = string.Empty;

            switch (user.Role)
            {
                case "Lecturer":
                    var lecturer = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.UserId == user.UserId);
                    if (lecturer != null)
                    {
                        roleId = lecturer.Id;
                        userName = lecturer.Name;
                    }
                    break;

                case "Coordinator":
                    var coordinator = await _context.Coordinators
                        .FirstOrDefaultAsync(c => c.UserId == user.UserId);
                    if (coordinator != null)
                    {
                        roleId = coordinator.Id;
                        userName = coordinator.Name;
                    }
                    break;

                case "Manager":
                    var manager = await _context.Managers
                        .FirstOrDefaultAsync(m => m.UserId == user.UserId);
                    if (manager != null)
                    {
                        roleId = manager.Id;
                        userName = manager.Name;
                    }
                    break;

                case "HR":
                    var hr = await _context.HRStaff
                        .FirstOrDefaultAsync(h => h.UserId == user.UserId);
                    if (hr != null)
                    {
                        roleId = hr.Id;
                        userName = hr.Name;
                    }
                    break;
            }

            // Store in session
            HttpContext.Session.SetInt32("UserId", roleId);
            HttpContext.Session.SetInt32("AccountUserId", user.UserId);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserName", userName);
            HttpContext.Session.SetString("Username", user.Username);

            // Redirect based on role
            return user.Role switch
            {
                "Lecturer" => RedirectToAction("Index", "Lecturer"),
                "Coordinator" => RedirectToAction("Index", "Coordinator"),
                "Manager" => RedirectToAction("Index", "Manager"),
                "HR" => RedirectToAction("Index", "HR"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Index");
        }
    }
}