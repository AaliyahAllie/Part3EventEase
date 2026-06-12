using Part3EventEase.Models;
using System.ComponentModel.DataAnnotations;

namespace Part3EventEase.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; }

        public ICollection<User>? Users { get; set; }
    }
}
