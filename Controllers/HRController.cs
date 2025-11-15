using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Attributes;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;

namespace ST10448895_CMCS_PROG.Controllers
{
    [AuthorizeRole("HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        // HR DASHBOARD - FIXED: Manual loading of modules
        public async Task<IActionResult> Index()
        {
            var lecturers = await _context.Lecturers.ToListAsync();

            // Manually load module count for each lecturer
            foreach (var lecturer in lecturers)
            {
                var moduleCount = await _context.LecturerModules
                    .CountAsync(lm => lm.LecturerId == lecturer.Id);

                // Store count in ViewBag or create a simple list
                ViewData[$"ModuleCount_{lecturer.Id}"] = moduleCount;
            }

            ViewBag.HRName = HttpContext.Session.GetString("UserName");
            return View(lecturers);
        }

        // CREATE LECTURER
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

        // EDIT LECTURER
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

        // LECTURER DETAILS - FIXED: Manual loading
        public async Task<IActionResult> LecturerDetails(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
                return NotFound();

            // Load modules separately
            var modules = await _context.LecturerModules
                .Where(lm => lm.LecturerId == id)
                .ToListAsync();

            // Load module details for each assignment
            foreach (var lm in modules)
            {
                lm.Module = await _context.Modules.FindAsync(lm.ModuleId);
            }

            ViewBag.AssignedModules = modules;
            return View(lecturer);
        }

        // DELETE LECTURER
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

        // MODULES LIST
        public async Task<IActionResult> Modules()
        {
            var modules = await _context.Modules.ToListAsync();
            return View(modules);
        }

        // CREATE MODULE
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

        // ASSIGN MODULE FORM
        public async Task<IActionResult> AssignModule()
        {
            ViewBag.Lecturers = await _context.Lecturers.ToListAsync();
            ViewBag.Modules = await _context.Modules.ToListAsync();
            return View();
        }

        // ASSIGN MODULE - SUBMIT
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

        // UPDATE RATE FORM
        public async Task<IActionResult> UpdateRate(int id)
        {
            var assignment = await _context.LecturerModules.FindAsync(id);
            if (assignment == null)
                return NotFound();

            // Load lecturer and module details
            assignment.Lecturer = await _context.Lecturers.FindAsync(assignment.LecturerId);
            assignment.Module = await _context.Modules.FindAsync(assignment.ModuleId);

            return View(assignment);
        }

        // UPDATE RATE - SUBMIT
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

        // REMOVE ASSIGNMENT
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