namespace ST10448895_CMCS_PROG.Models.ViewModels
{
    public class ReportsViewModel
    {
        public int TotalClaims { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ApprovedAmount { get; set; }

        public List<MonthlyReportData> ClaimsByMonth { get; set; } = new();
    }

    public class MonthlyReportData
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
