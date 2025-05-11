using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class Book
    {

        public long BookId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string BookPhoto { get; set; }

        [Required]
        public string Format { get; set; }

        [Required]
        public string Publisher { get; set; }

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public bool IsAvailableInLibrary { get; set; }

        public bool OnSale { get; set; } = false;

        [Range(0, 100)]
        public int? DiscountPercentage { get; set; }

        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        public string? ExclusiveEdition { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();


    }
}
