using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class Announce
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsMarked { get; set; } = false;

        public DateTime AnnouncemnetDateTime { get; set; }
        public DateTime AnnouncementEndDateTime { get; set; }
        public bool IsAnnounced { get; set; } = false;
    }
}
