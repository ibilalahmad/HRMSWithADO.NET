using Microsoft.AspNetCore.Mvc;
using AhmadHRManagementSystem.Repository;
using AhmadHRManagementSystem.Data;

namespace AhmadHRManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;

        public AccountController(IConfiguration configuration)
        {
            _userRepository = new UserRepository(configuration);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string identifier, string password)
        {
            var user = _userRepository.ValidateUser(identifier, password);

            if (user == null)
            {
                ViewBag.Error = "Invalid username/email or password.";
                return View();
            }

            // Store User Information in Session
            HttpContext.Session.SetString("Username", user["Username"].ToString());
            HttpContext.Session.SetString("FirstName", user["FirstName"].ToString());
            HttpContext.Session.SetString("Role", user["RoleName"].ToString());

            // Redirect based on RoleName
            return user["RoleName"].ToString() switch
            {
                "Admin" => RedirectToAction("Index", "Home"),
                "User" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Login", "Account", new { error = "Unauthorized role." })
            };
        }

        // Logout: Clear Session
        [AuthorizeRole("Admin", "User")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }

}
