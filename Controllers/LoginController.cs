using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;

namespace ST10448895_CMCS_PROG.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Clear any existing session
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Login model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check which role the user selected and verify from DB
            switch (model.Role)
            {
                case "Lecturer":
                    var lecturer = _context.Lecturers.FirstOrDefault(l => l.Name == model.Name);
                    if (lecturer == null)
                    {
                        ModelState.AddModelError("", "Lecturer not found.");
                        return View(model);
                    }

                    HttpContext.Session.SetString("UserRole", "Lecturer");
                    HttpContext.Session.SetString("UserName", lecturer.Name);
                    HttpContext.Session.SetInt32("UserId", lecturer.Id);
                    return RedirectToAction("Index", "Lecturer");

                case "Coordinator":
                    var coordinator = _context.Coordinators.FirstOrDefault(c => c.Name == model.Name);
                    if (coordinator == null)
                    {
                        ModelState.AddModelError("", "Coordinator not found.");
                        return View(model);
                    }

                    HttpContext.Session.SetString("UserRole", "Coordinator");
                    HttpContext.Session.SetString("UserName", coordinator.Name);
                    HttpContext.Session.SetInt32("UserId", coordinator.Id);
                    return RedirectToAction("Index", "Coordinator");

                case "Manager":
                    var manager = _context.Managers.FirstOrDefault(m => m.Name == model.Name);
                    if (manager == null)
                    {
                        ModelState.AddModelError("", "Manager not found.");
                        return View(model);
                    }

                    HttpContext.Session.SetString("UserRole", "Manager");
                    HttpContext.Session.SetString("UserName", manager.Name);
                    HttpContext.Session.SetInt32("UserId", manager.Id);
                    return RedirectToAction("Index", "Manager");

                default:
                    ModelState.AddModelError("", "Invalid role selected.");
                    return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
