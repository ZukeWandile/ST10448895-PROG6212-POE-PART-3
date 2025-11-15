using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("UploadDocuments")]
    public class UploadDocumentModel
    {
        public int Id { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        [StringLength(255)]
        public string Filename { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OriginalFilename { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [ForeignKey("ClaimId")]
        public virtual ClaimModel? Claim { get; set; }
    }
}
