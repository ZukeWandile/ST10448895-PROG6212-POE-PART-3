using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ST10448895_CMCS_PROG.Controllers;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace ST10448895_CMCS_PROG.Tests
{
    public class LecturerControllerTests
    {
        private LecturerController GetControllerWithContext(out ApplicationDbContext context)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "LecturerTestDB_" + System.Guid.NewGuid().ToString())
                .Options;

            context = new ApplicationDbContext(options);

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(env => env.WebRootPath).Returns(System.IO.Path.GetTempPath());

            var controller = new LecturerController(context, mockEnv.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Set up a mock session
            var sessionMock = new Mock<ISession>();
            var sessionValues = new Dictionary<string, byte[]>();
            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, val) => sessionValues[key] = val);
            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Callback(new TryGetValueCallback((string key, out byte[] value) =>
                {
                    sessionValues.TryGetValue(key, out value);
                }));

            controller.HttpContext.Session = sessionMock.Object;

            return controller;
        }

        private delegate void TryGetValueCallback(string key, out byte[] value);

        [Fact]
        public void Index_ReturnsRedirect_WhenUserIdNotInSession()
        {
            // Arrange
            var controller = GetControllerWithContext(out var context);

            // Act
            var result = controller.Index() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Login", result.ControllerName);
        }

        [Fact]
        public void Index_ReturnsViewResult_WhenUserIdInSession()
        {
            // Arrange
            var controller = GetControllerWithContext(out var context);

            // Mock session data
            controller.HttpContext.Session.SetInt32("UserId", 1);
            controller.HttpContext.Session.SetString("UserName", "Sam Suleck");

            // Add mock data
            context.Lecturers.Add(new LecturerModel { Id = 1, Name = "Sam Suleck", Email = "Sammy@example.com" });
            context.Claims.Add(new ClaimModel { Id = 1, LecturerId = 1, Description = "Test Claim", HoursWorked = 5, HourlyRate = 200 });
            context.SaveChanges();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            Assert.True(result.Model is List<ClaimModel>);
            var claims = result.Model as List<ClaimModel>;
            Assert.Single(claims);
        }

        [Fact]
        public void Submit_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = GetControllerWithContext(out var _);

            // Act
            var result = controller.Submit() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
        }
    }
}
