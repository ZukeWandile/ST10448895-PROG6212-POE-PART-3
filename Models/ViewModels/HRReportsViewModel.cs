
namespace ST10448895_CMCS_PROG.Models.ViewModels
{
    public class HRReportsViewModel
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int VerifiedClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public List<ClaimSummary> RecentClaims { get; set; } = new List<ClaimSummary>();
        public List<LecturerSummary> TopLecturers { get; set; } = new List<LecturerSummary>();
    }

    public class ClaimSummary
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmitDate { get; set; }
    }

    public class LecturerSummary
    {
        public string LecturerName { get; set; } = string.Empty;
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
    }
}