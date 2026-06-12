using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Part3EventEase.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "Event type name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<Event>? Events { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}

