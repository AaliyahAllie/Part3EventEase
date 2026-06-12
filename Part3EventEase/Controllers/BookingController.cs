using Part3EventEase.Data;
using Part3EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Part3EventEase.Controllers
{
    public class BookingController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingController(EventEaseContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard(string? search, string? status, DateTime? startDate, DateTime? endDate, int? eventTypeId, int? venueId, bool? venueAvailable)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.User)
                .Include(b => b.EventType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                bookings = bookings.Where(b =>
                    b.PersonName.Contains(search) ||
                    b.ContactDetails.Contains(search) ||
                    b.Venue.VenueName.Contains(search) ||
                    b.User.Name.Contains(search) ||
                    (b.EventType != null && b.EventType.Name.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                bookings = bookings.Where(b => b.Status == status);

            if (startDate.HasValue)
                bookings = bookings.Where(b => b.BookingDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                bookings = bookings.Where(b => b.BookingDate.Date <= endDate.Value.Date);

            if (eventTypeId.HasValue)
                bookings = bookings.Where(b => b.EventTypeId == eventTypeId.Value);

            if (venueId.HasValue)
                bookings = bookings.Where(b => b.VenueId == venueId.Value);

            if (venueAvailable.HasValue)
                bookings = bookings.Where(b => b.Venue.IsAvailable == venueAvailable.Value);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.EventTypeId = eventTypeId;
            ViewBag.VenueId = venueId;
            ViewBag.VenueAvailable = venueAvailable;

            ViewBag.EventTypes = _context.EventTypes.OrderBy(et => et.Name).ToList();
            ViewBag.Venues = _context.Venues.OrderBy(v => v.VenueName).ToList();

            ViewBag.TotalBookings = bookings.Count();
            ViewBag.PendingBookings = bookings.Count(b => b.Status == "Pending");
            ViewBag.ApprovedBookings = bookings.Count(b => b.Status == "Approved");
            ViewBag.DeniedBookings = bookings.Count(b => b.Status == "Denied");

            return View(bookings.OrderByDescending(b => b.BookingDate).ToList());
        }

        public IActionResult Create(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Customer")
                return RedirectToAction("Login", "Account");

            var venue = _context.Venues.Find(id);
            if (venue == null) return NotFound();

            ViewBag.VenueName = venue.VenueName;
            ViewBag.EventTypes = _context.EventTypes.OrderBy(et => et.Name).ToList();

            return View(new Booking
            {
                VenueId = id,
                BookingDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Booking booking)
        {
            if (HttpContext.Session.GetString("UserRole") != "Customer")
                return RedirectToAction("Login", "Account");

            ModelState.Remove("Venue");
            ModelState.Remove("User");
            ModelState.Remove("Event");
            ModelState.Remove("EventType");

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            booking.UserId = int.Parse(userIdString);

            var venue = _context.Venues.Find(booking.VenueId);
            if (venue == null) return NotFound();

            ViewBag.VenueName = venue.VenueName;

            var selectedDate = booking.BookingDate.Date;

            var hasApprovedEvent = _context.Events.Any(e =>
                e.VenueId == booking.VenueId &&
                e.EventDate.Date == selectedDate &&
                e.Status == "Approved");

            var hasActiveBooking = _context.Bookings.Any(b =>
                b.VenueId == booking.VenueId &&
                b.BookingDate.Date == selectedDate &&
                b.Status != "Denied");

            if (hasApprovedEvent || hasActiveBooking)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
                ViewBag.EventTypes = _context.EventTypes.OrderBy(et => et.Name).ToList();
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                booking.Status = "Pending";

                _context.Bookings.Add(booking);
                _context.SaveChanges();

                TempData["Success"] = "Booking submitted successfully. Please wait for admin approval.";
                return RedirectToAction("MyBookings");
            }

            ViewBag.EventTypes = _context.EventTypes.OrderBy(et => et.Name).ToList();
            return View(booking);
        }

        public IActionResult MyBookings()
        {
            if (HttpContext.Session.GetString("UserRole") != "Customer")
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var bookings = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.EventType)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var booking = _context.Bookings
                .Include(b => b.Venue)
                .FirstOrDefault(b => b.BookingId == id);

            if (booking == null) return NotFound();

            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only pending bookings can be approved.";
                return RedirectToAction("Dashboard");
            }

            var selectedDate = booking.BookingDate.Date;

            var hasApprovedEvent = _context.Events.Any(e =>
                e.VenueId == booking.VenueId &&
                e.EventDate.Date == selectedDate &&
                e.Status == "Approved");

            if (hasApprovedEvent)
            {
                TempData["Error"] = "Cannot approve this booking because an approved event already exists for this venue on this date.";
                return RedirectToAction("Dashboard");
            }

            var newEvent = new Event
            {
                EventName = $"Event for {booking.PersonName}",
                EventDate = booking.BookingDate,
                VenueId = booking.VenueId,
                CustomerName = booking.PersonName,
                Description = $"Auto-created from booking by {booking.PersonName}. Contact: {booking.ContactDetails}",
                Status = "Pending",
                EventTypeId = booking.EventTypeId
            };

            _context.Events.Add(newEvent);
            _context.SaveChanges();

            booking.EventId = newEvent.EventId;
            booking.Status = "Approved";

            _context.SaveChanges();

            TempData["Success"] = "Booking approved and event created.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deny(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var booking = _context.Bookings.Find(id);
            if (booking == null) return NotFound();

            booking.Status = "Denied";
            _context.SaveChanges();

            TempData["Success"] = "Booking denied.";
            return RedirectToAction("Dashboard");
        }
    }
}
