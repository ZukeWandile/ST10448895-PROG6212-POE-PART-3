using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class LecturerModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Lecturer Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<ClaimModel> Claims { get; set; } = new List<ClaimModel>();
    }
}