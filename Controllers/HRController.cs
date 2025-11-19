using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Attributes;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using ST10448895_CMCS_PROG.Models.ViewModels;
using ST10448895_CMCS_PROG.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

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


        // HR DASHBOARD
        public async Task<IActionResult> Index()
        {
            // Get only lecturers with active user accounts
            var activeLecturers = await (from l in _context.Lecturers
                                         join u in _context.Users on l.UserId equals u.UserId
                                         where u.IsActive
                                         select l)
                                        .ToListAsync();

            if (activeLecturers.Any())
            {
                // Get all lecturer IDs
                var lecturerIds = activeLecturers.Select(l => l.Id).ToList();

                // Load all module counts in one query
                var moduleCounts = await _context.LecturerModules
                    .Where(lm => lecturerIds.Contains(lm.LecturerId))
                    .GroupBy(lm => lm.LecturerId)
                    .Select(g => new { LecturerId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Store in ViewData
                foreach (var lecturer in activeLecturers)
                {
                    var count = moduleCounts.FirstOrDefault(mc => mc.LecturerId == lecturer.Id)?.Count ?? 0;
                    ViewData[$"ModuleCount_{lecturer.Id}"] = count;
                }
            }

            ViewBag.HRName = HttpContext.Session.GetString("UserName");
            return View(activeLecturers);
        }

        // VIEW: Manage Users
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userDetails = new List<dynamic>();

            foreach (var user in users)
            {
                string fullName = "";
                string email = "";

                switch (user.Role)
                {
                    case "Lecturer":
                        var lecturer = await _context.Lecturers
                            .FirstOrDefaultAsync(l => l.UserId == user.UserId);
                        fullName = lecturer?.Name ?? "";
                        email = lecturer?.Email ?? "";
                        break;

                    case "Coordinator":
                        var coord = await _context.Coordinators
                            .FirstOrDefaultAsync(c => c.UserId == user.UserId);
                        fullName = coord?.Name ?? "";
                        break;

                    case "Manager":
                        var manager = await _context.Managers
                            .FirstOrDefaultAsync(m => m.UserId == user.UserId);
                        fullName = manager?.Name ?? "";
                        break;

                    case "HR":
                        var hr = await _context.HRStaff
                            .FirstOrDefaultAsync(h => h.UserId == user.UserId);
                        fullName = hr?.Name ?? "";
                        email = hr?.Email ?? "";
                        break;
                }

                userDetails.Add(new
                {
                    user.UserId,
                    user.Username,
                    user.Role,
                    FullName = fullName,
                    Email = email,
                    user.CreatedAt,
                    user.LastLogin
                });
            }

            ViewBag.UserDetails = userDetails;
            return View(users);
        }

        // GET: Create User
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Username check
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            // Email validation - only required for Lecturer and HR
            if (model.Role == "Lecturer" || model.Role == "HR")
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", $"Email is required for {model.Role} role.");
                    return View(model);
                }

                bool emailExists = model.Role == "Lecturer"
                    ? await _context.Lecturers.AnyAsync(l => l.Email == model.Email)
                    : await _context.HRStaff.AnyAsync(h => h.Email == model.Email);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }
            }

            var hrUserId = HttpContext.Session.GetInt32("AccountUserId") ?? 0;

            try
            {
                // Create system user
                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = PasswordHasher.HashPassword(model.Password),
                    Role = model.Role,
                    CreatedBy = hrUserId,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific record
                switch (model.Role)
                {
                    case "Lecturer":
                        var lecturer = new LecturerModel
                        {
                            UserId = user.UserId,
                            Name = model.FullName,
                            Email = model.Email!, 
                            Department = model.Department
                        };
                        _context.Lecturers.Add(lecturer);
                        break;

                    case "Coordinator":
                        var coordinator = new CoordinatorModel
                        {
                            UserId = user.UserId,
                            Name = model.FullName
                        };
                        _context.Coordinators.Add(coordinator);
                        break;

                    case "Manager":
                        var manager = new ManagerModel
                        {
                            UserId = user.UserId,
                            Name = model.FullName
                        };
                        _context.Managers.Add(manager);
                        break;

                    case "HR":
                        var hr = new HR
                        {
                            UserId = user.UserId,
                            Name = model.FullName,
                            Email = model.Email! 
                        };
                        _context.HRStaff.Add(hr);
                        break;
                }

                await _context.SaveChangesAsync();

                // Store credentials to display (for this session only)
                TempData["NewUsername"] = model.Username;
                TempData["NewPassword"] = model.Password;
                TempData["NewRole"] = model.Role;
                TempData["Success"] = $"{model.Role} account created successfully!";

                return RedirectToAction("CreateUser");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating user: {ex.Message}";
                return View(model);
            }
        }
        // POST: Deactivate User
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "User deactivated successfully.";
            return RedirectToAction("ManageUsers");
        }
        // LECTURER CRUD
        public IActionResult CreateLecturer() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLecturer(LecturerModel lecturer)
        {
            if (!ModelState.IsValid)
                return View(lecturer);

            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecturer created successfully!";
            return RedirectToAction("Index");
        }

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

            if (!ModelState.IsValid)
                return View(lecturer);

            _context.Update(lecturer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecturer updated successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LecturerDetails(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null)
                return NotFound();

            var modules = await _context.LecturerModules
                .Where(m => m.LecturerId == id)
                .ToListAsync();

            foreach (var m in modules)
                m.Module = await _context.Modules.FindAsync(m.ModuleId);

            ViewBag.AssignedModules = modules;
            return View(lecturer);
        }

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

        // MODULES & ASSIGNMENTS

        public async Task<IActionResult> Modules()
        {
            return View(await _context.Modules.ToListAsync());
        }

        public IActionResult CreateModule() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModule(Module module)
        {
            if (!ModelState.IsValid)
                return View(module);

            _context.Modules.Add(module);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Module created successfully!";
            return RedirectToAction("Modules");
        }

        public async Task<IActionResult> AssignModule()
        {
            // Only show active lecturers
            var activeLecturers = await (from l in _context.Lecturers
                                         join u in _context.Users on l.UserId equals u.UserId
                                         where u.IsActive
                                         select l)
                                        .ToListAsync();

            ViewBag.Lecturers = activeLecturers;
            ViewBag.Modules = await _context.Modules.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignModule(int lecturerId, int moduleId, decimal hourlyRate)
        {
            if (await _context.LecturerModules.AnyAsync(lm => lm.LecturerId == lecturerId && lm.ModuleId == moduleId))
            {
                TempData["Error"] = "This lecturer is already assigned to this module!";
                ViewBag.Lecturers = await _context.Lecturers.ToListAsync();
                ViewBag.Modules = await _context.Modules.ToListAsync();
                return View();
            }

            _context.LecturerModules.Add(new LecturerModule
            {
                LecturerId = lecturerId,
                ModuleId = moduleId,
                HourlyRate = hourlyRate
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Module assigned successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UpdateRate(int id)
        {
            var assignment = await _context.LecturerModules.FindAsync(id);
            if (assignment == null)
                return NotFound();

            assignment.Lecturer = await _context.Lecturers.FindAsync(assignment.LecturerId);
            assignment.Module = await _context.Modules.FindAsync(assignment.ModuleId);

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

        // VIEW: Reports Dashboard
        public async Task<IActionResult> Reports()
        {
            var claims = await _context.Claims.ToListAsync();
            var lecturers = await _context.Lecturers.ToListAsync();

            var model = new HRReportsViewModel
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => !c.Verified && c.Status != "Rejected"),
                VerifiedClaims = claims.Count(c => c.Verified && !c.Approved),
                ApprovedClaims = claims.Count(c => c.Approved),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                TotalAmount = claims.Sum(c => c.HoursWorked * c.HourlyRate),
                ApprovedAmount = claims.Where(c => c.Approved).Sum(c => c.HoursWorked * c.HourlyRate),
                PendingAmount = claims.Where(c => !c.Verified && c.Status != "Rejected").Sum(c => c.HoursWorked * c.HourlyRate),
                RecentClaims = claims.OrderByDescending(c => c.SubmitDate).Take(10).Select(c => new ClaimSummary
                {
                    ClaimId = c.Id,
                    LecturerName = lecturers.FirstOrDefault(l => l.Id == c.LecturerId)?.Name ?? "Unknown",
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    TotalAmount = c.HoursWorked * c.HourlyRate,
                    Status = c.Status,
                    SubmitDate = c.SubmitDate
                }).ToList(),
                TopLecturers = claims.Where(c => c.Approved)
                    .GroupBy(c => c.LecturerId)
                    .Select(g => new LecturerSummary
                    {
                        LecturerName = lecturers.FirstOrDefault(l => l.Id == g.Key)?.Name ?? "Unknown",
                        TotalClaims = g.Count(),
                        TotalAmount = g.Sum(c => c.HoursWorked * c.HourlyRate)
                    })
                    .OrderByDescending(l => l.TotalAmount)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }

        // VIEW: All Claims
        public async Task<IActionResult> AllClaims()
        {
            var claims = await _context.Claims.OrderByDescending(c => c.SubmitDate).ToListAsync();
            var lecturers = await _context.Lecturers.ToListAsync();

            var claimsList = claims.Select(c => new ClaimSummary
            {
                ClaimId = c.Id,
                LecturerName = lecturers.FirstOrDefault(l => l.Id == c.LecturerId)?.Name ?? "Unknown",
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                TotalAmount = c.HoursWorked * c.HourlyRate,
                Status = c.Status,
                SubmitDate = c.SubmitDate
            }).ToList();

            return View(claimsList);
        }

        // DOWNLOAD: PDF Report , All Claims
        public async Task<IActionResult> DownloadAllClaimsPDF()
        {
            var claims = await _context.Claims.OrderByDescending(c => c.SubmitDate).ToListAsync();
            var lecturers = await _context.Lecturers.ToListAsync();

            using (var memoryStream = new MemoryStream())
            {
                // Create PDF document
                var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate());
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Title
                var titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD);
                var title = new iTextSharp.text.Paragraph("All Claims Report", titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                title.SpacingAfter = 20f;
                document.Add(title);

                // Date
                var dateFont = iTextSharp.text.FontFactory.GetFont("Arial", 10);
                var date = new iTextSharp.text.Paragraph($"Generated: {DateTime.Now:dd MMMM yyyy HH:mm}", dateFont);
                date.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                date.SpacingAfter = 20f;
                document.Add(date);

                // Summary
                var summaryFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD);
                document.Add(new iTextSharp.text.Paragraph("Summary", summaryFont));
                document.Add(new iTextSharp.text.Paragraph($"Total Claims: {claims.Count}", dateFont));
                document.Add(new iTextSharp.text.Paragraph($"Total Amount: R{claims.Sum(c => c.HoursWorked * c.HourlyRate):N2}", dateFont));
                document.Add(new iTextSharp.text.Paragraph($"Approved: {claims.Count(c => c.Approved)}", dateFont));
                document.Add(new iTextSharp.text.Paragraph($"Pending: {claims.Count(c => !c.Verified && c.Status != "Rejected")}", dateFont));
                document.Add(new iTextSharp.text.Paragraph($"Rejected: {claims.Count(c => c.Status == "Rejected")}", dateFont));
                document.Add(new iTextSharp.text.Paragraph(" "));

                // Table
                var table = new iTextSharp.text.pdf.PdfPTable(7);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 25f, 15f, 15f, 15f, 15f, 15f });

                // Headers
                var headerFont = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD);
                table.AddCell(new iTextSharp.text.Phrase("ID", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Lecturer", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Hours", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Rate", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Total", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Status", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Date", headerFont));

                // Data
                var cellFont = iTextSharp.text.FontFactory.GetFont("Arial", 9);
                foreach (var claim in claims)
                {
                    var lecturer = lecturers.FirstOrDefault(l => l.Id == claim.LecturerId);
                    table.AddCell(new iTextSharp.text.Phrase(claim.Id.ToString(), cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(lecturer?.Name ?? "Unknown", cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.HoursWorked.ToString(), cellFont));
                    table.AddCell(new iTextSharp.text.Phrase($"R{claim.HourlyRate:N2}", cellFont));
                    table.AddCell(new iTextSharp.text.Phrase($"R{(claim.HoursWorked * claim.HourlyRate):N2}", cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.Status, cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.SubmitDate.ToString("dd/MM/yyyy"), cellFont));
                }

                document.Add(table);
                document.Close();

                var bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", $"AllClaims_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        // DOWNLOAD: PDF Invoice for Approved Claims
        public async Task<IActionResult> DownloadApprovedInvoice()
        {
            var approvedClaims = await _context.Claims.Where(c => c.Approved).OrderBy(c => c.SubmitDate).ToListAsync();
            var lecturers = await _context.Lecturers.ToListAsync();

            using (var memoryStream = new MemoryStream())
            {
                var document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4);
                var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Title
                var titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 20, iTextSharp.text.Font.BOLD);
                var title = new iTextSharp.text.Paragraph("PAYMENT INVOICE", titleFont);
                title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                document.Add(title);

                // Invoice Details
                var normalFont = iTextSharp.text.FontFactory.GetFont("Arial", 10);
                document.Add(new iTextSharp.text.Paragraph($"Invoice Number: INV-{DateTime.Now:yyyyMMdd}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Invoice Date: {DateTime.Now:dd MMMM yyyy}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Period: All Approved Claims", normalFont));
                document.Add(new iTextSharp.text.Paragraph(" "));

                // Table
                var table = new iTextSharp.text.pdf.PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 30f, 15f, 15f, 15f, 15f });

                // Headers
                var headerFont = iTextSharp.text.FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD);
                table.AddCell(new iTextSharp.text.Phrase("ID", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Lecturer", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Hours", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Rate", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Amount", headerFont));
                table.AddCell(new iTextSharp.text.Phrase("Date", headerFont));

                // Data
                var cellFont = iTextSharp.text.FontFactory.GetFont("Arial", 9);
                decimal totalAmount = 0;
                foreach (var claim in approvedClaims)
                {
                    var lecturer = lecturers.FirstOrDefault(l => l.Id == claim.LecturerId);
                    var amount = claim.HoursWorked * claim.HourlyRate;
                    totalAmount += amount;

                    table.AddCell(new iTextSharp.text.Phrase(claim.Id.ToString(), cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(lecturer?.Name ?? "Unknown", cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.HoursWorked.ToString(), cellFont));
                    table.AddCell(new iTextSharp.text.Phrase($"R{claim.HourlyRate:N2}", cellFont));
                    table.AddCell(new iTextSharp.text.Phrase($"R{amount:N2}", cellFont));
                    table.AddCell(new iTextSharp.text.Phrase(claim.SubmitDate.ToString("dd/MM/yyyy"), cellFont));
                }

                document.Add(table);

                // Total
                document.Add(new iTextSharp.text.Paragraph(" "));
                var totalFont = iTextSharp.text.FontFactory.GetFont("Arial", 14, iTextSharp.text.Font.BOLD);
                var totalPara = new iTextSharp.text.Paragraph($"TOTAL AMOUNT DUE: R{totalAmount:N2}", totalFont);
                totalPara.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                document.Add(totalPara);

                // Footer
                document.Add(new iTextSharp.text.Paragraph(" "));
                document.Add(new iTextSharp.text.Paragraph(" "));
                var footerFont = iTextSharp.text.FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.ITALIC);
                document.Add(new iTextSharp.text.Paragraph("Please process payment within the next month.", footerFont));

                document.Close();

                var bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", $"Invoice_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }
    }
}
