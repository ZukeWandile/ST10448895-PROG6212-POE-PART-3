using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("LecturerModules")]
    public class LecturerModule
    {
        public int Id { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(50, 5000)]
        public decimal HourlyRate { get; set; }

        [ForeignKey("LecturerId")]
        public virtual LecturerModel? Lecturer { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module? Module { get; set; }
    }
}