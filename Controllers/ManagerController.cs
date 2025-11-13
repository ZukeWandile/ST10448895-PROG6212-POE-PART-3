using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using ST10448895_CMCS_PROG.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace ST10448895_CMCS_PROG.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("CMCS1234CMCS1234");

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsManager()
        {
            return HttpContext.Session.GetString("UserRole") == "Manager";
        }

        public IActionResult Index()
        {
            if (!IsManager())
                return RedirectToAction("Index", "Login");

            var dashboard = BuildManagerDashboard();
            ViewBag.ManagerName = HttpContext.Session.GetString("UserName") ?? "Manager";
            return View(dashboard);
        }

        public IActionResult Approve()
        {
            if (!IsManager())
                return RedirectToAction("Index", "Login");

            var claims = _context.Claims
                .Include(c => c.Documents)
                .Where(c => c.Verified && !c.Approved)
                .OrderByDescending(c => c.SubmitDate)
                .ToList();

            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveClaim(int claimId, string action, string? notes)
        {
            if (!IsManager())
                return RedirectToAction("Index", "Login");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == claimId);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approve");
            }

            if (action == "approve")
            {
                claim.Approved = true;
                claim.Status = "Approved";
            }
            else if (action == "reject")
            {
                claim.Approved = false;
                claim.Status = "Rejected";
            }

            if (!string.IsNullOrWhiteSpace(notes))
                claim.Description += $"\n[Manager Notes: {notes}]";

            _context.SaveChanges();
            TempData["Success"] = $"Claim #{claimId} {(action == "approve" ? "approved" : "rejected")} successfully!";
            return RedirectToAction("Approve");
        }

        public IActionResult Reports()
        {
            if (!IsManager())
                return RedirectToAction("Index", "Login");

            var claims = _context.Claims.ToList();

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


        // Secure download + decryption
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

        private DashboardViewModel BuildManagerDashboard()
        {
            var claims = _context.Claims.ToList();

            return new DashboardViewModel
            {
                TotalClaims = claims.Count,
                PendingClaims = claims.Count(c => c.Verified && !c.Approved),
                ApprovedClaims = claims.Count(c => c.Approved),
                RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                TotalAmount = claims.Sum(c => c.TotalAmount),
                PendingAmount = claims.Where(c => c.Verified && !c.Approved).Sum(c => c.TotalAmount),
                RecentClaims = claims.OrderByDescending(c => c.SubmitDate).Take(5).ToList()
            };
        }
    }
}
