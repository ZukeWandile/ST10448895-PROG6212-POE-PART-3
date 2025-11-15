using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("Modules")]
    public class Module
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string ModuleCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ModuleName { get; set; } = string.Empty;

        [NotMapped]
        public virtual ICollection<LecturerModule> LecturerModules { get; set; } = new List<LecturerModule>();
    }
}