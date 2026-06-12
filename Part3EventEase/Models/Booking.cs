using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Part3EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        public int? EventId { get; set; }
        public Event? Event { get; set; }

        [Required]
        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string PersonName { get; set; }

        [Required]
        public string ContactDetails { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        public string Status { get; set; } = "Pending";
    }
}
