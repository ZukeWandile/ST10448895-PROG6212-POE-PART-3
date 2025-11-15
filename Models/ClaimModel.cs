using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("Claims")]
    public class ClaimModel
    {
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int HoursWorked { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000)]
        public decimal HourlyRate { get; set; }

        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime SubmitDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";

        [Required]
        public bool Verified { get; set; } = false;

        [Required]
        public bool Approved { get; set; } = false;

        // Navigation properties - DO NOT create FK columns
        [ForeignKey("LecturerId")]
        public virtual LecturerModel? Lecturer { get; set; }

        [NotMapped]
        public virtual ICollection<UploadDocumentModel> Documents { get; set; } = new List<UploadDocumentModel>();

        [NotMapped]
        public virtual ApprovalModel? Approval { get; set; }
    }
}