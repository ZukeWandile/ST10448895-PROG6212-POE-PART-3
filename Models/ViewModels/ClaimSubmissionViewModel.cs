namespace ST10448895_CMCS_PROG.Models.ViewModels
{
    public class ClaimSubmissionViewModel
    {
        public ClaimModel Claim { get; set; } = new ClaimModel();
        public List<IFormFile> Documents { get; set; } = new List<IFormFile>();
    }
}