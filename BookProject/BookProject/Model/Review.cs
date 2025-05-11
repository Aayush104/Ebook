using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class Review
    {

        [Key]
        public int Id { get; set; }

        public string Comment { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? Star { get; set; }

        [Required]
        public long BookId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey(nameof(BookId))]
        public virtual Book? Book { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
