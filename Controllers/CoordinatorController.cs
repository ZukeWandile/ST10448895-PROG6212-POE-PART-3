// Controllers/CoordinatorController.cs - FIXED
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Attributes;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using ST10448895_CMCS_PROG.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace ST10448895_CMCS_PROG.Controllers
{
    [AuthorizeRole("Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("CMCS1234CMCS1234");

        public CoordinatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public async Task<IActionResult> Index()
        {
            var dashboard = await BuildCoordinatorDashboard();
            ViewBag.CoordinatorName = HttpContext.Session.GetString("UserName") ?? "Coordinator";
            return View(dashboard);
        }

        // VERIFY CLAIMS - FIXED: Removed .Include()
        public async Task<IActionResult> Verify()
        {
            // Get pending claims without navigation properties
            var pendingClaims = await _context.Claims
                .Where(c => !c.Verified)
                .OrderByDescending(c => c.SubmitDate)
                .ToListAsync();

            // Manually load documents for each claim
            foreach (var claim in pendingClaims)
            {
                var documents = await _context.UploadDocuments
                    .Where(d => d.ClaimId == claim.Id)
                    .ToListAsync();

                // Store in ViewBag for each claim
                ViewData[$"Documents_{claim.Id}"] = documents;
            }

            return View(pendingClaims);
        }

        // VERIFY OR REJECT CLAIM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyClaim(int claimId, string action, string? notes)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Verify");
            }

            var coordinatorId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (action == "verify")
            {
                claim.Verified = true;
                claim.Status = "Verified";

                // Create or update approval record
                var approval = await _context.Approvals
                    .FirstOrDefaultAsync(a => a.ClaimId == claimId);

                if (approval == null)
                {
                    approval = new ApprovalModel
                    {
                        ClaimId = claimId,
                        CoordinatorId = coordinatorId,
                        DateVerified = DateTime.Now,
                        VerificationNotes = notes
                    };
                    _context.Approvals.Add(approval);
                }
                else
                {
                    approval.CoordinatorId = coordinatorId;
                    approval.DateVerified = DateTime.Now;
                    approval.VerificationNotes = notes;
                }
            }
            else if (action == "reject")
            {
                claim.Verified = false;
                claim.Status = "Rejected";

                // Create rejection record
                var approval = new ApprovalModel
                {
                    ClaimId = claimId,
                    CoordinatorId = coordinatorId,
                    DateVerified = DateTime.Now,
                    VerificationNotes = notes ?? "Rejected by coordinator"
                };
                _context.Approvals.Add(approval);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Claim #{claimId} {(action == "verify" ? "verified" : "rejected")} successfully!";
            return RedirectToAction("Verify");
        }

        // DOWNLOAD ENCRYPTED DOCUMENT
        public IActionResult DownloadDocument(int id)
        {
            var document = _context.UploadDocuments.FirstOrDefault(d => d.Id == id);
            if (document == null)
                return NotFound();

            try
            {
                using var aes = Aes.Create();
                aes.Key = EncryptionKey;
                aes.IV = new byte[16];

                using var inputStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
                using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var memoryStream = new MemoryStream();
                cryptoStream.CopyTo(memoryStream);

                return File(memoryStream.ToArray(), document.ContentType, document.OriginalFilename);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error downloading file.";
                return RedirectToAction("Verify");
            }
        }

        // BUILD DASHBOARD DATA
        private async Task<DashboardViewModel> BuildCoordinatorDashboard()
        {
            var claims = await _context.Claims.ToListAsync();

            return new DashboardViewModel
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => !c.Verified),
                VerifiedClaims = claims.Count(c => c.Verified),
                ApprovedClaims = claims.Count(c => c.Approved),
                TotalAmount = claims.Sum(c => c.HoursWorked * c.HourlyRate),
                PendingAmount = claims.Where(c => !c.Verified).Sum(c => c.HoursWorked * c.HourlyRate),
                RecentClaims = claims.OrderByDescending(c => c.SubmitDate).Take(5).ToList()
            };
        }
    }
}