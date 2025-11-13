using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class UploadDocumentModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Claim")]
        public int ClaimId { get; set; }

        [Required]
        [Display(Name = "File Name")]
        [StringLength(255)]
        public string Filename { get; set; } = string.Empty;

        [Display(Name = "Original File Name")]
        [StringLength(255)]
        public string OriginalFilename { get; set; } = string.Empty;

        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        [Display(Name = "Content Type")]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Display(Name = "Upload Date")]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [Display(Name = "File Path")]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        // Navigation property
        public virtual ClaimModel? Claim { get; set; }
    }
}