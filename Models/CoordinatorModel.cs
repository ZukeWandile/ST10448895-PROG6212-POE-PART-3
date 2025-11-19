using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10448895_CMCS_PROG.Models
{
    [Table("Coordinators")]
    public class CoordinatorModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation property 
        [NotMapped]
        public virtual ICollection<ClaimModel> ClaimsToVerify { get; set; } = new List<ClaimModel>();
    }
}