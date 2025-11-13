using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Select Role")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Your Name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
    }
}