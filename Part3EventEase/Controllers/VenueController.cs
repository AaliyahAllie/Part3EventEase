using Part3EventEase.Data;
using Part3EventEase.Models;
using Part3EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Part3EventEase.Controllers
{
    public class VenueController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly BlobService _blobService;

        public VenueController(EventEaseContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public IActionResult Index(string? search, string? location, int? minCapacity, DateTime? startDate, DateTime? endDate, bool? availableOnly)
        {
            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                venues = venues.Where(v => v.VenueName.Contains(search));

            if (!string.IsNullOrWhiteSpace(location))
                venues = venues.Where(v => v.Location.Contains(location));

            if (minCapacity.HasValue)
                venues = venues.Where(v => v.Capacity >= minCapacity.Value);

            if (availableOnly.HasValue && availableOnly.Value)
            {
                venues = venues.Where(v => v.IsAvailable);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                venues = venues.Where(v => !v.Bookings.Any(b => 
                    b.Status == "Approved" && 
                    b.BookingDate.Date >= startDate.Value.Date && 
                    b.BookingDate.Date <= endDate.Value.Date));
            }

            ViewBag.Search = search;
            ViewBag.Location = location;
            ViewBag.MinCapacity = minCapacity;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.AvailableOnly = availableOnly;
            ViewBag.VenueCount = venues.Count();

            return View(venues.ToList());
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var imageUrl = await _blobService.UploadFileAsync(imageFile);
                venue.ImageUrl = imageUrl ?? "/images/placeholder.jpg";

                _context.Venues.Add(venue);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Venue created successfully.";
                return RedirectToAction("Index");
            }

            return View(venue);
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var venue = _context.Venues.Find(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Venue venue, IFormFile? imageFile)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var existingVenue = _context.Venues.Find(venue.VenueId);
            if (existingVenue == null) return NotFound();

            if (ModelState.IsValid)
            {
                existingVenue.VenueName = venue.VenueName;
                existingVenue.Location = venue.Location;
                existingVenue.Capacity = venue.Capacity;
                existingVenue.IsAvailable = venue.IsAvailable;

                var imageUrl = await _blobService.UploadFileAsync(imageFile);
                if (!string.IsNullOrEmpty(imageUrl))
                    existingVenue.ImageUrl = imageUrl;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Venue updated successfully.";
                return RedirectToAction("Index");
            }

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");

            var hasActiveBookings = _context.Bookings.Any(b =>
                b.VenueId == id &&
                b.Status != "Denied");

            var hasEvents = _context.Events.Any(e => e.VenueId == id);

            if (hasActiveBookings || hasEvents)
            {
                TempData["Error"] = "Cannot delete this venue because it has active bookings or events.";
                return RedirectToAction("Index");
            }

            var venue = _context.Venues.Find(id);
            if (venue == null) return NotFound();

            _context.Venues.Remove(venue);
            _context.SaveChanges();

            TempData["Success"] = "Venue deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
