using Microsoft.AspNetCore.Identity;
using static System.Net.WebRequestMethods;
using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }

        public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();



    }
}
