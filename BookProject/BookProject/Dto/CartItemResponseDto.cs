namespace BookProject.Dto
{
    public class CartItemResponseDto
    {
        public long BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
