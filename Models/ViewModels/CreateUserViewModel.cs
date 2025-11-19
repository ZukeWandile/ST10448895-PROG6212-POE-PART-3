
using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

       
        [EmailAddress]
        public string? Email { get; set; }

        // Department is only for Lecturers
        public string? Department { get; set; }
    }
}