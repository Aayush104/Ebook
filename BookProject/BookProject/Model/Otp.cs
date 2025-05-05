using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class Otp
    {
        [Key]
        public int Id { get; set; }
        public string IsOtp { get; set; }


        public bool IsUsed { get; set; }

        [StringLength(50)]
        public string Purpose { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;



        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
