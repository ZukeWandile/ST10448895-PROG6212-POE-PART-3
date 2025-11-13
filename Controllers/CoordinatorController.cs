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

        private bool IsCoordinator()
        {
            return HttpContext.Session.GetString("UserRole") == "Coordinator";
        }

        public IActionResult Index()
        {
            if (!IsCoordinator())
                return RedirectToAction("Index", "Login");

            var dashboard = BuildCoordinatorDashboard();
            ViewBag.CoordinatorName = HttpContext.Session.GetString("UserName") ?? "Coordinator";
            return View(dashboard);
        }

        public IActionResult Verify()
        {
            if (!IsCoordinator())
                return RedirectToAction("Index", "Login");

            var pendingClaims = _context.Claims
                .Include(c => c.Documents)
                .Where(c => !c.Verified)
                .OrderByDescending(c => c.SubmitDate)
                .ToList();

            return View(pendingClaims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyClaim(int claimId, string action, string? notes)
        {
            if (!IsCoordinator())
                return RedirectToAction("Index", "Login");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == claimId);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Verify");
            }

            if (action == "verify")
            {
                claim.Verified = true;
                claim.Status = "Verified";
            }
            else if (action == "reject")
            {
                claim.Verified = false;
                claim.Status = "Rejected";
            }

            if (!string.IsNullOrWhiteSpace(notes))
                claim.Description += $"\n[Coordinator Notes: {notes}]";

            _context.SaveChanges();
            TempData["Success"] = $"Claim #{claimId} {(action == "verify" ? "verified" : "rejected")} successfully!";
            return RedirectToAction("Verify");
        }

        //   Download encrypted file (decrypts before returning)
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

        private DashboardViewModel BuildCoordinatorDashboard()
        {
            var claims = _context.Claims.ToList();

            return new DashboardViewModel
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => !c.Verified),
                VerifiedClaims = claims.Count(c => c.Verified),
                ApprovedClaims = claims.Count(c => c.Approved),
                TotalAmount = claims.Sum(c => c.TotalAmount),
                PendingAmount = claims.Where(c => !c.Verified).Sum(c => c.TotalAmount),
                RecentClaims = claims.OrderByDescending(c => c.SubmitDate).Take(5).ToList()
            };
        }
    }
}
