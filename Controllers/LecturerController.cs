using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using ST10448895_CMCS_PROG.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace ST10448895_CMCS_PROG.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private readonly string[] AllowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
        private readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("CMCS1234CMCS1234"); // 16 bytes AES key

        public LecturerController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // LECTURER DASHBOARD (Claims)
        public IActionResult Index()
        {
            var lecturerId = HttpContext.Session.GetInt32("UserId");
            var lecturerName = HttpContext.Session.GetString("UserName");

            if (lecturerId == null)
                return RedirectToAction("Index", "Login");

            var claims = _context.Claims
                .Where(c => c.LecturerId == lecturerId)
                .Include(c => c.Documents)
                .ToList();

            ViewBag.LecturerName = lecturerName;
            return View(claims);
        }
        // SUBMIT CLAIM FORM
        public IActionResult Submit()
        {
            return View(new ClaimSubmissionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ClaimSubmissionViewModel model, List<IFormFile> Documents)
        {
            var lecturerId = HttpContext.Session.GetInt32("UserId");

            if (lecturerId == null)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
                return View(model);

            var claim = new ClaimModel
            {
                LecturerId = lecturerId.Value,
                Description = model.Claim.Description,
                HoursWorked = model.Claim.HoursWorked,
                HourlyRate = model.Claim.HourlyRate,
                SubmitDate = DateTime.Now,
                Status = "Pending",
                Verified = false,
                Approved = false
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // DOCUMENT UPLOAD WITH ENCRYPTION

            if (Documents != null && Documents.Any())
            {
                string uploadDir = Path.Combine(_environment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadDir);

                foreach (var file in Documents)
                {
                    string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!AllowedExtensions.Contains(ext))
                    {
                        TempData["Error"] = $"Invalid file type: {file.FileName}";
                        continue;
                    }

                    if (file.Length > MaxFileSize)
                    {
                        TempData["Error"] = $"File too large: {file.FileName}";
                        continue;
                    }

                    string encryptedFilePath = Path.Combine(uploadDir, Guid.NewGuid() + ext);
                    using (var fileStream = new FileStream(encryptedFilePath, FileMode.Create))
                    {
                        using var aes = Aes.Create();
                        aes.Key = EncryptionKey;
                        aes.IV = new byte[16];
                        using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                        await file.CopyToAsync(cryptoStream);
                    }

                    var doc = new UploadDocumentModel
                    {
                        ClaimId = claim.Id,
                        Filename = Path.GetFileName(encryptedFilePath),
                        OriginalFilename = file.FileName,
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        FilePath = encryptedFilePath,
                        UploadedAt = DateTime.Now
                    };

                    _context.UploadDocuments.Add(doc);
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Claim submitted successfully!";
            return RedirectToAction(nameof(Index));
        }
        // VIEW DOCUMENTS PER CLAIM
        public IActionResult Documents(int claimId)
        {
            var documents = _context.UploadDocuments
                .Where(d => d.ClaimId == claimId)
                .ToList();

            ViewBag.ClaimId = claimId;
            return View(documents);
        }

        // DOWNLOAD AND DECRYPT DOCUMENT
        public IActionResult DownloadDocument(int id)
        {
            var document = _context.UploadDocuments.FirstOrDefault(d => d.Id == id);
            if (document == null)
                return NotFound();

            using var aes = Aes.Create();
            aes.Key = EncryptionKey;
            aes.IV = new byte[16];

            using var inputStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
            using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var memoryStream = new MemoryStream();
            cryptoStream.CopyTo(memoryStream);

            return File(memoryStream.ToArray(), document.ContentType, document.OriginalFilename);
        }

        // TRACK CLAIM STATUS
     
        public IActionResult Track(int id)
        {
            var claim = _context.Claims
                .Include(c => c.Documents)
                .FirstOrDefault(c => c.Id == id);

            if (claim == null)
                return NotFound();

            return View(claim);
        }
    }
}
