using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AhmadHRManagementSystem.Data
{
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles ?? Array.Empty<string>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var role = session.GetString("Role");

            // Check if user is not logged in (no role found)
            if (string.IsNullOrEmpty(role))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { error = "Please log in to continue." });
                return;
            }

            // Check if user's role is not allowed (case-insensitive comparison)
            if (!_roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { message = "You do not have permission to access this page." });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
