using Part3EventEase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Part3EventEase.Controllers
{
    public class EventController : Controller
    {
        private readonly EventEaseContext _context;

        public EventController(EventEaseContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? search, string? status, DateTime? startDate, DateTime? endDate, int? eventTypeId, int? venueId, bool? venueAvailable)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                events = events.Where(e =>
                    e.EventName.Contains(search) ||
                    e.CustomerName.Contains(search) ||
                    e.Venue.VenueName.Contains(search) ||
                    (e.EventType != null && e.EventType.Name.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                events = events.Where(e => e.Status == status);

            if (startDate.HasValue)
                events = events.Where(e => e.EventDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                events = events.Where(e => e.EventDate.Date <= endDate.Value.Date);

            if (eventTypeId.HasValue)
                events = events.Where(e => e.EventTypeId == eventTypeId.Value);

            if (venueId.HasValue)
                events = events.Where(e => e.VenueId == venueId.Value);

            if (venueAvailable.HasValue)
                events = events.Where(e => e.Venue.IsAvailable == venueAvailable.Value);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.EventTypeId = eventTypeId;
            ViewBag.VenueId = venueId;
            ViewBag.VenueAvailable = venueAvailable;

            ViewBag.EventTypes = _context.EventTypes.OrderBy(et => et.Name).ToList();
            ViewBag.Venues = _context.Venues.OrderBy(v => v.VenueName).ToList();

            return View(events.OrderByDescending(e => e.EventDate).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound();

            ev.Status = "Approved";
            _context.SaveChanges();

            TempData["Success"] = "Event approved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deny(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound();

            ev.Status = "Denied";
            _context.SaveChanges();

            TempData["Success"] = "Event denied.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var hasActiveBookings = _context.Bookings.Any(b =>
                b.EventId == id &&
                b.Status != "Denied");

            if (hasActiveBookings)
            {
                TempData["Error"] = "Cannot delete this event because it is linked to an active booking.";
                return RedirectToAction("Index");
            }

            var ev = _context.Events.Find(id);
            if (ev == null) return NotFound();

            _context.Events.Remove(ev);
            _context.SaveChanges();

            TempData["Success"] = "Event deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
