using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class ManagerModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Manager Name")]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<ClaimModel> ClaimsToApprove { get; set; } = new List<ClaimModel>();
    }
}