using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ST10448895_CMCS_PROG.Controllers;
using ST10448895_CMCS_PROG.Data;
using ST10448895_CMCS_PROG.Models;
using Xunit;

namespace ST10448895_CMCS_PROG.Tests
{
    public class LoginControllerTests
    {
        private LoginController GetController(ApplicationDbContext context)
        {
            return new LoginController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = HttpContextHelper.CreateHttpContextWithSession()
                }
            };
        }

        [Fact]
        public void Index_Get_ClearsSessionAndReturnsView()
        {
            var context = TestDbContextHelper.GetInMemoryDbContext();
            var controller = GetController(context);

            controller.HttpContext.Session.SetString("UserName", "Lecturer");

            var result = controller.Index() as ViewResult;

            Assert.NotNull(result);
        }
    }
}
