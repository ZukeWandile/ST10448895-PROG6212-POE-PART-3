// File: TestDbContextHelper.cs
using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Data;

namespace ST10448895_CMCS_PROG.Tests
{
    public static class TestDbContextHelper
    {
        public static ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
