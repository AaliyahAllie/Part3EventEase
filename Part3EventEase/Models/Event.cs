using Part3EventEase.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Part3EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string? Description { get; set; }

        public string? CustomerName { get; set; }

        public string Status { get; set; } = "Pending";

        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}
