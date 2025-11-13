using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Attributes;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;

namespace ST10448895_CMCS_PROG.Controllers
{
    [AuthorizeRole("HR")] // SECURITY: Only HR can access this controller
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        // HR Dashboard
        public async Task<IActionResult> Index()
        {
            var lecturers = await _context.Lecturers
                .Include(l => l.LecturerModules)
                .ThenInclude(lm => lm.Module)
                .ToListAsync();

            ViewBag.HRName = HttpContext.Session.GetString("UserName");
            return View(lecturers);
        }

        // Create Lecturer
        public IActionResult CreateLecturer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLecturer(LecturerModel lecturer)
        {
            if (ModelState.IsValid)
            {
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lecturer created successfully!";
                return RedirectToAction("Index");
            }

            return View(lecturer);
        }

        // Edit Lecturer
        public async Task<IActionResult> EditLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
                return NotFound();

            return View(lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(int id, LecturerModel lecturer)
        {
            if (id != lecturer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(lecturer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lecturer updated successfully!";
                return RedirectToAction("Index");
            }

            return View(lecturer);
        }

        // View Lecturer Details
        public async Task<IActionResult> LecturerDetails(int id)
        {
            var lecturer = await _context.Lecturers
                .Include(l => l.LecturerModules)
                .ThenInclude(lm => lm.Module)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecturer == null)
                return NotFound();

            return View(lecturer);
        }

        // Delete Lecturer
        [HttpPost]
        public async Task<IActionResult> DeleteLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
                return NotFound();

            _context.Lecturers.Remove(lecturer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecturer deleted.";
            return RedirectToAction("Index");
        }

        // MODULE MANAGEMENT

        public async Task<IActionResult> Modules()
        {
            var modules = await _context.Modules.ToListAsync();
            return View(modules);
        }

        public IActionResult CreateModule()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModule(Module module)
        {
            if (ModelState.IsValid)
            {
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Module created successfully!";
                return RedirectToAction("Modules");
            }

            return View(module);
        }

        // ASSIGN MODULE & RATE 

        public async Task<IActionResult> AssignModule()
        {
            ViewBag.Lecturers = await _context.Lecturers.ToListAsync();
            ViewBag.Modules = await _context.Modules.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignModule(int lecturerId, int moduleId, decimal hourlyRate)
        {
            // Check if already assigned
            var exists = await _context.LecturerModules
                .AnyAsync(lm => lm.LecturerId == lecturerId && lm.ModuleId == moduleId);

            if (exists)
            {
                TempData["Error"] = "This lecturer is already assigned to this module!";
                ViewBag.Lecturers = await _context.Lecturers.ToListAsync();
                ViewBag.Modules = await _context.Modules.ToListAsync();
                return View();
            }

            var assignment = new LecturerModule
            {
                LecturerId = lecturerId,
                ModuleId = moduleId,
                HourlyRate = hourlyRate
            };

            _context.LecturerModules.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Module assigned successfully!";
            return RedirectToAction("Index");
        }

        // Update Rate
        public async Task<IActionResult> UpdateRate(int id)
        {
            var assignment = await _context.LecturerModules
                .Include(lm => lm.Lecturer)
                .Include(lm => lm.Module)
                .FirstOrDefaultAsync(lm => lm.Id == id);

            if (assignment == null)
                return NotFound();

            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRate(int id, decimal hourlyRate)
        {
            var assignment = await _context.LecturerModules.FindAsync(id);
            if (assignment == null)
                return NotFound();

            assignment.HourlyRate = hourlyRate;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rate updated successfully!";
            return RedirectToAction("LecturerDetails", new { id = assignment.LecturerId });
        }

        // Remove Assignment
        [HttpPost]
        public async Task<IActionResult> RemoveAssignment(int id)
        {
            var assignment = await _context.LecturerModules.FindAsync(id);
            if (assignment == null)
                return NotFound();

            _context.LecturerModules.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Assignment removed.";
            return RedirectToAction("LecturerDetails", new { id = assignment.LecturerId });
        }
    }
}