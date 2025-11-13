using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class CoordinatorModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Coordinator Name")]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<ClaimModel> ClaimsToVerify { get; set; } = new List<ClaimModel>();
    }
}