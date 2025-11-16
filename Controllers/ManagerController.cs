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
    [AuthorizeRole("Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("CMCS1234CMCS1234");

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public async Task<IActionResult> Index()
        {
            var dashboard = await BuildManagerDashboard();
            ViewBag.ManagerName = HttpContext.Session.GetString("UserName") ?? "Manager";
            return View(dashboard);
        }

        // APPROVE CLAIMS 
        public async Task<IActionResult> Approve()
        {
            // Get verified claims that are pending manager approval (not rejected)
            var verifiedClaims = await _context.Claims
                .Where(c => c.Verified && !c.Approved && c.Status != "Rejected")
                .OrderByDescending(c => c.SubmitDate)
                .ToListAsync();

            // Get all documents for these claims in one query
            var claimIds = verifiedClaims.Select(c => c.Id).ToList();
            var allDocuments = await _context.UploadDocuments
                .Where(d => claimIds.Contains(d.ClaimId))
                .ToListAsync();

            // Create a dictionary for easy lookup in the view
            var documentsByClaim = allDocuments
                .GroupBy(d => d.ClaimId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.DocumentsByClaim = documentsByClaim;

            return View(verifiedClaims);
        }

        // APPROVE OR REJECT CLAIM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int claimId, string action, string? notes)
        {
            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.Id == claimId);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approve");
            }

            var managerId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (action == "approve")
            {
                claim.Approved = true;
                claim.Status = "Approved";

                // Update approval record with manager approval
                var approval = await _context.Approvals
                    .FirstOrDefaultAsync(a => a.ClaimId == claimId);

                if (approval != null)
                {
                    approval.ManagerId = managerId;
                    approval.DateApproved = DateTime.Now;
                    approval.ApprovalNotes = notes;
                }
            }
            else if (action == "reject")
            {
                claim.Approved = false;
                claim.Status = "Rejected";

                // Update approval record with manager rejection
                var approval = await _context.Approvals
                    .FirstOrDefaultAsync(a => a.ClaimId == claimId);

                if (approval != null)
                {
                    approval.ManagerId = managerId;
                    approval.DateApproved = DateTime.Now;
                    approval.ApprovalNotes = notes ?? "Rejected by manager";
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Claim #{claimId} {(action == "approve" ? "approved" : "rejected")} successfully!";
            return RedirectToAction("Approve");
        }

        // REPORTS
        public async Task<IActionResult> Reports()
        {
            var claims = await _context.Claims.ToListAsync();

            var model = new ReportsViewModel
            {
                TotalClaims = claims.Count,
                TotalAmount = claims.Sum(c => c.TotalAmount),
                ApprovedAmount = claims.Where(c => c.Approved).Sum(c => c.TotalAmount),
                ClaimsByMonth = claims
                    .GroupBy(c => c.SubmitDate.ToString("MMMM yyyy"))
                    .Select(g => new MonthlyReportData
                    {
                        Month = g.Key,
                        Count = g.Count(),
                        Amount = g.Sum(c => c.TotalAmount)
                    })
                    .OrderByDescending(m => DateTime.Parse("01 " + m.Month))
                    .ToList()
            };

            return View(model);
        }

        // DOWNLOAD ENCRYPTED DOCUMENT
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.UploadDocuments.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                TempData["Error"] = "Document not found.";
                return RedirectToAction("Approve");
            }

            // Check if file exists
            if (!System.IO.File.Exists(document.FilePath))
            {
                TempData["Error"] = "File not found on server.";
                return RedirectToAction("Approve");
            }

            try
            {
                using var aes = Aes.Create();
                aes.Key = EncryptionKey;
                aes.IV = new byte[16];

                using var inputStream = new FileStream(document.FilePath, FileMode.Open, FileAccess.Read);
                using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var memoryStream = new MemoryStream();
                cryptoStream.CopyTo(memoryStream);

                var decryptedData = memoryStream.ToArray();
                return File(decryptedData, document.ContentType, document.OriginalFilename);
            }
            catch (CryptographicException ex)
            {
                TempData["Error"] = "Error decrypting file. The file may be corrupted.";
                return RedirectToAction("Approve");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error downloading file.";
                return RedirectToAction("Approve");
            }
        }

        // BUILD DASHBOARD DATA
        private async Task<DashboardViewModel> BuildManagerDashboard()
        {
            var claims = await _context.Claims.ToListAsync();

            return new DashboardViewModel
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Verified && !c.Approved && c.Status != "Rejected"),
                ApprovedClaims = claims.Count(c => c.Approved),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                TotalAmount = claims.Sum(c => c.TotalAmount),
                PendingAmount = claims.Where(c => c.Verified && !c.Approved && c.Status != "Rejected").Sum(c => c.TotalAmount),
                RecentClaims = claims.OrderByDescending(c => c.SubmitDate).Take(5).ToList()
            };
        }
    }
}