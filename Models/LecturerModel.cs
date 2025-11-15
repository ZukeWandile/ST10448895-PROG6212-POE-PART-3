using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("Lecturers")]
    public class LecturerModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Department { get; set; }

        // Navigation properties 
        [NotMapped]
        public virtual ICollection<ClaimModel> Claims { get; set; } = new List<ClaimModel>();

        [NotMapped]
        public virtual ICollection<LecturerModule> LecturerModules { get; set; } = new List<LecturerModule>();
    }
}