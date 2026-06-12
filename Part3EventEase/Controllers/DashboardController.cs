using Part3EventEase.Data;
using Microsoft.AspNetCore.Mvc;

namespace Part3EventEase.Controllers
{
    public class DashboardController : Controller
    {
        private readonly EventEaseContext _context;

        public DashboardController(EventEaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.TotalVenues = _context.Venues.Count();

            ViewBag.TotalBookings = _context.Bookings.Count();
            ViewBag.PendingBookings = _context.Bookings.Count(b => b.Status == "Pending");
            ViewBag.ApprovedBookings = _context.Bookings.Count(b => b.Status == "Approved");
            ViewBag.DeniedBookings = _context.Bookings.Count(b => b.Status == "Denied");

            ViewBag.TotalEvents = _context.Events.Count();
            ViewBag.PendingEvents = _context.Events.Count(e => e.Status == "Pending");
            ViewBag.ApprovedEvents = _context.Events.Count(e => e.Status == "Approved");
            ViewBag.DeniedEvents = _context.Events.Count(e => e.Status == "Denied");

            return View();
        }
    }
}
