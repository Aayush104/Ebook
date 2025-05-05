namespace BookProject.Dto
{
     public class UpdateBookDto
    {
        public string Title { get; set; }
        public string ISBN { get; set; }
        public IFormFile? BookPhotoFile { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Language { get; set; }
        public string Format { get; set; }
        public string Publisher { get; set; }
        public DateTime PublicationDate { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsAvailableInLibrary { get; set; }
        public bool OnSale { get; set; }
        public int? DiscountPercentage { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public string? ExclusiveEdition { get; set; }
    }
}
