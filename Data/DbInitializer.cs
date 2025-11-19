using Microsoft.AspNetCore.Identity;
using ST10448895_CMCS_PROG.Models;
using ST10448895_CMCS_PROG.Helpers;

namespace ST10448895_CMCS_PROG.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if already seeded
            if (context.Users.Any())
            {
                return; // DB has been seeded
            }

            // Create default admin user
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                Role = "HR",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            // Create HR Staff record for admin
            var hrStaff = new HR
            {
                UserId = adminUser.UserId,
                Name = "HR Admin",
                Email = "hradmin@university.edu"
            };
            context.HRStaff.Add(hrStaff);

            // Add sample modules
            var modules = new Module[]
            {
                new Module { ModuleCode = "PROG6212", ModuleName = "Programming 2B" },
                new Module { ModuleCode = "DATABASE", ModuleName = "Database Design" },
                new Module { ModuleCode = "WEB101", ModuleName = "Web Development" },
                new Module { ModuleCode = "MOBILE", ModuleName = "Mobile Development" }
            };
            context.Modules.AddRange(modules);
            context.SaveChanges();

            // Create sample lecturer user
            var lecturerUser = new User
            {
                Username = "Bolt",
                PasswordHash = PasswordHasher.HashPassword("Lecturer123!"),
                Role = "Lecturer",
                CreatedBy = adminUser.UserId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(lecturerUser);
            context.SaveChanges();

            // Create lecturer record
            var lecturer = new LecturerModel
            {
                UserId = lecturerUser.UserId,
                Name = "Usain Bolt",
                Email = "Bolt.u@iiemsa.edu.za",
                Department = "Computer Science"
            };
            context.Lecturers.Add(lecturer);
            context.SaveChanges();

            // Create sample coordinator user
            var coordinatorUser = new User
            {
                Username = "Poki",
                PasswordHash = PasswordHasher.HashPassword("Coord123!"),
                Role = "Coordinator",
                CreatedBy = adminUser.UserId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(coordinatorUser);
            context.SaveChanges();

            // Create coordinator record
            var coordinator = new CoordinatorModel
            {
                UserId = coordinatorUser.UserId,
                Name = "Poki Mane"
            };
            context.Coordinators.Add(coordinator);
            context.SaveChanges();

            // Create sample manager user
            var managerUser = new User
            {
                Username = "Titan",
                PasswordHash = PasswordHasher.HashPassword("Manager123!"),
                Role = "Manager",
                CreatedBy = adminUser.UserId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Users.Add(managerUser);
            context.SaveChanges();

            // Create manager record
            var manager = new ManagerModel
            {
                UserId = managerUser.UserId,
                Name = "Eren Yeager"
            };
            context.Managers.Add(manager);
            context.SaveChanges();

            // Assign modules to lecturer
            var lecturerModules = new LecturerModule[]
            {
                new LecturerModule
                {
                    LecturerId = lecturer.Id,
                    ModuleId = modules[0].Id,
                    HourlyRate = 150.00m
                },
                new LecturerModule
                {
                    LecturerId = lecturer.Id,
                    ModuleId = modules[1].Id,
                    HourlyRate = 150.00m
                }
            };
            context.LecturerModules.AddRange(lecturerModules);
            context.SaveChanges();

            Console.WriteLine("Database seeded successfully!");
        }
    }
}