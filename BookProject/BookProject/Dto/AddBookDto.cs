using System.ComponentModel.DataAnnotations;

namespace BookProject.Dto
{
    public class AddBookDto
    {


        [Required]
        public string Title { get; set; }

        [Required]
        public string ISBN { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public IFormFile? BookPhotoFile { get; set; }


        public string? BookPhoto { get; set; }

        [Required]
        public string Genre { get; set; }

        [Required]
        public string Language { get; set; }

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


    }

}