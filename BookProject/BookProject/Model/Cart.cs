using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public long BookId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey(nameof(BookId))]
        public virtual Book? Book { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}
