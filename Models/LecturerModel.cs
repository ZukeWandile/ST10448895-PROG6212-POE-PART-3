using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class LecturerModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Department { get; set; }

        public virtual ICollection<ClaimModel> Claims { get; set; } = new List<ClaimModel>();
        public virtual ICollection<LecturerModule> LecturerModules { get; set; } = new List<LecturerModule>();
    }
}