using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ST10448895_CMCS_PROG.Controllers;
using ST10448895_CMCS_PROG.Data;
using Microsoft.AspNetCore.Http;
using ST10448895_CMCS_PROG.Models;
using Xunit;

namespace ST10448895_CMCS_PROG.Tests
{
    public class CoordinatorControllerTests
    {
        private CoordinatorController GetController(ApplicationDbContext context)
        {
            var controller = new CoordinatorController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = HttpContextHelper.CreateHttpContextWithSession()
                }
            };

            controller.HttpContext.Session.SetString("UserRole", "Coordinator");
            controller.HttpContext.Session.SetString("UserName", "TestCoordinator");

            return controller;
        }

        [Fact]
        public void Index_ReturnsViewResult_WithModel()
        {
            var context = TestDbContextHelper.GetInMemoryDbContext();
            var controller = GetController(context);

            var result = controller.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

       /* [Fact]
        public void VerifyClaim_InvalidId_RedirectsToIndex()
        {
            var context = TestDbContextHelper.GetInMemoryDbContext();
            var controller = GetController(context);

            var result = controller.VerifyClaim(0,null,null) as RedirectToActionResult;

            Assert.Equal("Index", result.ActionName);
        }*/
    }
}
