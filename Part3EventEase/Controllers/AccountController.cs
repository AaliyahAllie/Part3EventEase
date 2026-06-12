
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Part3EventEase.Controllers
{
    public class AccountController : Controller
    {
        private readonly EventEaseContext _context;

        public AccountController(EventEaseContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserRole", user.Role.RoleName);
            HttpContext.Session.SetString("UserName", user.Name);

            if (user.Role.RoleName == "Admin")
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("Index", "Venue");
        }

        public IActionResult Register()
        {
            ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "RoleName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string name, string email, string password, int roleId)
        {
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "This email is already registered.";
                ViewBag.Roles = new SelectList(_context.Roles.ToList(), "RoleId", "RoleName");
                return View();
            }

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = roleId
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Account created successfully. Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
