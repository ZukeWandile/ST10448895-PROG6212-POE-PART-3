
using System.ComponentModel.DataAnnotations;

namespace ST10448895_CMCS_PROG.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;
    }
}