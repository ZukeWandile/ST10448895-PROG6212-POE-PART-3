namespace ST10448895_CMCS_PROG.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int VerifiedClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public List<ClaimModel> RecentClaims { get; set; } = new List<ClaimModel>();
    }
}