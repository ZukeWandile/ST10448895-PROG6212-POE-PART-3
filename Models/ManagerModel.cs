using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("Managers")]
    public class ManagerModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        [NotMapped]
        public virtual ICollection<ClaimModel> ClaimsToApprove { get; set; } = new List<ClaimModel>();
    }
}