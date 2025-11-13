using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    public class ClaimModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Lecturer")]
        public int LecturerId { get; set; }

        [Required]
        [Display(Name = "Hours Worked")]
        [Range(1, 1000, ErrorMessage = "Hours must be between 1 and 1000")]
        public int HoursWorked { get; set; }

        [Required]
        [Display(Name = "Hourly Rate")]
        [Range(0.01, 10000, ErrorMessage = "Hourly rate must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Submit Date")]
        public DateTime SubmitDate { get; set; } = DateTime.Now;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Submitted";

        [Display(Name = "Document")]
        public string? Document { get; set; }

        [Display(Name = "Verified")]
        public bool Verified { get; set; } = false;

        [Display(Name = "Approved")]
        public bool Approved { get; set; } = false;

        // Navigation properties
        public virtual LecturerModel? Lecturer { get; set; }
        public virtual ICollection<UploadDocumentModel> Documents { get; set; } = new List<UploadDocumentModel>();
        public virtual ApprovalModel? Approval { get; set; }
    }
}