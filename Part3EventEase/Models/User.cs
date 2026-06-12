using Part3EventEase.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Part3EventEase.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }

        public ICollection<Booking>? Bookings { get; set; }
    }
}
