using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ST10448895_CMCS_PROG.Attributes
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}