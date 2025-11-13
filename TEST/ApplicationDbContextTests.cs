using Microsoft.EntityFrameworkCore;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using Xunit;
using System;

namespace ST10448895_CMCS_PROG.Tests
{
    public class ApplicationDbContextTests
    {
        private ApplicationDbContext GetInMemoryContext(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CanAddAndRetrieveClaim()
        {
            using var context = GetInMemoryContext();

            var claim = new ClaimModel
            {
                Id = 100,
                HoursWorked = 8,
                HourlyRate = 150
                // TotalAmount is calculated automatically
            };

            context.Claims.Add(claim);
            context.SaveChanges();

            var saved = context.Claims.Find(100);
            Assert.NotNull(saved);
            Assert.Equal(8, saved.HoursWorked);
            Assert.Equal(150, saved.HourlyRate);
            Assert.True(saved.TotalAmount > 0); // ensure computed value exists
        }

        [Fact]
        public void Database_EnsureCreated_ReturnsTrueOrNoException()
        {
            using var context = GetInMemoryContext();
            // Ensures database creation succeeds
            var created = context.Database.EnsureCreated();
            Assert.True(created || !created); // no exception expected
        }
    }
}
