using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("Approvals")]
    public class ApprovalModel
    {
        public int Id { get; set; }

        [Required]
        public int ClaimId { get; set; }

        public int? CoordinatorId { get; set; }

        public int? ManagerId { get; set; }

        public DateTime? DateVerified { get; set; }

        public DateTime? DateApproved { get; set; }

        [StringLength(500)]
        public string? VerificationNotes { get; set; }

        [StringLength(500)]
        public string? ApprovalNotes { get; set; }

        [ForeignKey("ClaimId")]
        public virtual ClaimModel? Claim { get; set; }

        [ForeignKey("CoordinatorId")]
        public virtual CoordinatorModel? Coordinator { get; set; }

        [ForeignKey("ManagerId")]
        public virtual ManagerModel? Manager { get; set; }
    }
}