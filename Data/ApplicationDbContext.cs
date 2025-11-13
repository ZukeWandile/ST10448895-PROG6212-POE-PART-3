using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Models;

namespace ST10448895_CMCS_PROG.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LecturerModel> Lecturers { get; set; }
        public DbSet<CoordinatorModel> Coordinators { get; set; }
        public DbSet<ManagerModel> Managers { get; set; }
        public DbSet<ClaimModel> Claims { get; set; }
        public DbSet<ApprovalModel> Approvals { get; set; }
        public DbSet<UploadDocumentModel> UploadDocuments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Lecturers
            modelBuilder.Entity<LecturerModel>().HasData(
                new LecturerModel { Id = 1, Name = "John Smith", Email = "john.smith@college.edu" },
                new LecturerModel { Id = 2, Name = "Mary Johnson", Email = "mary.johnson@college.edu" }
            );

            // Seed Coordinators
            modelBuilder.Entity<CoordinatorModel>().HasData(
                new CoordinatorModel { Id = 1, Name = "Dr. Linda Brown" }
            );

            // Seed Managers
            modelBuilder.Entity<ManagerModel>().HasData(
                new ManagerModel { Id = 1, Name = "Prof. Alan White" }
            );

            // Seed a sample claim
            modelBuilder.Entity<ClaimModel>().HasData(
                new ClaimModel
                {
                    Id = 1,
                    LecturerId = 1,
                    HoursWorked = 5,
                    HourlyRate = 300,
                    Description = "Cybersecurity workshop claim",
                    SubmitDate = DateTime.Now,
                    Status = "Pending",
                    Verified = false,
                    Approved = false
                }
            );
        }
    }
}
