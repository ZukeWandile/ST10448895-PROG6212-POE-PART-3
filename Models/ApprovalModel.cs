using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class ApprovalModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Claim")]
        public int ClaimId { get; set; }

        [Display(Name = "Coordinator")]
        public int? CoordinatorId { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }

        [Display(Name = "Date Verified")]
        public DateTime? DateVerified { get; set; }

        [Display(Name = "Date Approved")]
        public DateTime? DateApproved { get; set; }

        [Display(Name = "Verification Notes")]
        [StringLength(500)]
        public string? VerificationNotes { get; set; }

        [Display(Name = "Approval Notes")]
        [StringLength(500)]
        public string? ApprovalNotes { get; set; }

        // Navigation properties
        public virtual ClaimModel? Claim { get; set; }
        public virtual CoordinatorModel? Coordinator { get; set; }
        public virtual ManagerModel? Manager { get; set; }
    }
}