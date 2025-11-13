
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
        public DbSet<HR> HRStaff { get; set; }
        public DbSet<ClaimModel> Claims { get; set; }
        public DbSet<ApprovalModel> Approvals { get; set; }
        public DbSet<UploadDocumentModel> UploadDocuments { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<LecturerModule> LecturerModules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relationships
            modelBuilder.Entity<ClaimModel>()
                .HasOne(c => c.Lecturer)
                .WithMany(l => l.Claims)
                .HasForeignKey(c => c.LecturerId);

            modelBuilder.Entity<LecturerModule>()
                .HasOne(lm => lm.Lecturer)
                .WithMany(l => l.LecturerModules)
                .HasForeignKey(lm => lm.LecturerId);

            modelBuilder.Entity<LecturerModule>()
                .HasOne(lm => lm.Module)
                .WithMany(m => m.LecturerModules)
                .HasForeignKey(lm => lm.ModuleId);
        }
    }
}