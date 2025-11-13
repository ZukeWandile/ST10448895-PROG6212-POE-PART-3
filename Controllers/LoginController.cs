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
        public async Task<IActionResult> Index(Login model)
        {
            if (ModelState.IsValid)
            {
                HttpContext.Session.SetString("UserRole", model.Role);
                HttpContext.Session.SetString("UserName", model.Name);

                int userId = 0;

                if (model.Role == "Lecturer")
                {
                    var lecturer = await _context.Lecturers
                        .FirstOrDefaultAsync(l => l.Name == model.Name);

                    if (lecturer == null)
                    {
                        TempData["Error"] = "Lecturer not found. Please contact HR.";
                        return View(model);
                    }

                    userId = lecturer.Id;
                }
                else if (model.Role == "Coordinator")
                {
                    var coordinator = await _context.Coordinators
                        .FirstOrDefaultAsync(c => c.Name == model.Name);

                    if (coordinator == null)
                    {
                        coordinator = new CoordinatorModel { Name = model.Name };
                        _context.Coordinators.Add(coordinator);
                        await _context.SaveChangesAsync();
                    }

                    userId = coordinator.Id;
                }
                else if (model.Role == "Manager")
                {
                    var manager = await _context.Managers
                        .FirstOrDefaultAsync(m => m.Name == model.Name);

                    if (manager == null)
                    {
                        manager = new ManagerModel { Name = model.Name };
                        _context.Managers.Add(manager);
                        await _context.SaveChangesAsync();
                    }

                    userId = manager.Id;
                }
                else if (model.Role == "HR")
                {
                    var hr = await _context.HRStaff
                        .FirstOrDefaultAsync(h => h.Name == model.Name);

                    if (hr == null)
                    {
                        hr = new HR
                        {
                            Name = model.Name,
                            Email = $"{model.Name.Replace(" ", "").ToLower()}@iieMSA.edu.za"
                        };
                        _context.HRStaff.Add(hr);
                        await _context.SaveChangesAsync();
                    }

                    userId = hr.Id;
                }

                HttpContext.Session.SetInt32("UserId", userId);

                return model.Role switch
                {
                    "Lecturer" => RedirectToAction("Index", "Lecturer"),
                    "Coordinator" => RedirectToAction("Index", "Coordinator"),
                    "Manager" => RedirectToAction("Index", "Manager"),
                    "HR" => RedirectToAction("Index", "HR"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
