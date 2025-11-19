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

        public DbSet<User> Users { get; set; }
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

            
            // Configure Users table
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            });

            // Configure Lecturer-User relationship
            modelBuilder.Entity<LecturerModel>(entity =>
            {
                entity.ToTable("Lecturers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<LecturerModel>(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Coordinator-User relationship
            modelBuilder.Entity<CoordinatorModel>(entity =>
            {
                entity.ToTable("Coordinators");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<CoordinatorModel>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Manager-User relationship
            modelBuilder.Entity<ManagerModel>(entity =>
            {
                entity.ToTable("Managers");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<ManagerModel>(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure HR-User relationship
            modelBuilder.Entity<HR>(entity =>
            {
                entity.ToTable("HRStaff");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne<User>()
                    .WithOne()
                    .HasForeignKey<HR>(h => h.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CLAIM RELATIONSHIPS 
            modelBuilder.Entity<ClaimModel>()
                .HasOne(c => c.Lecturer)
                .WithMany()
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LecturerModule>()
                .HasOne(lm => lm.Lecturer)
                .WithMany()
                .HasForeignKey(lm => lm.LecturerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LecturerModule>()
                .HasOne(lm => lm.Module)
                .WithMany()
                .HasForeignKey(lm => lm.ModuleId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UploadDocumentModel>()
                .HasOne(u => u.Claim)
                .WithMany()
                .HasForeignKey(u => u.ClaimId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApprovalModel>()
                .HasOne(a => a.Claim)
                .WithOne()
                .HasForeignKey<ApprovalModel>(a => a.ClaimId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApprovalModel>()
                .HasOne(a => a.Coordinator)
                .WithMany()
                .HasForeignKey(a => a.CoordinatorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApprovalModel>()
                .HasOne(a => a.Manager)
                .WithMany()
                .HasForeignKey(a => a.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            
            modelBuilder.Entity<LecturerModel>()
                .Ignore(l => l.Claims)
                .Ignore(l => l.LecturerModules);

            modelBuilder.Entity<CoordinatorModel>()
                .Ignore(c => c.ClaimsToVerify);

            modelBuilder.Entity<ManagerModel>()
                .Ignore(m => m.ClaimsToApprove);

            modelBuilder.Entity<ClaimModel>()
                .Ignore(c => c.Documents)
                .Ignore(c => c.Approval)
                .Ignore(c => c.TotalAmount);

            modelBuilder.Entity<Module>()
                .Ignore(m => m.LecturerModules);
        }
    }
}