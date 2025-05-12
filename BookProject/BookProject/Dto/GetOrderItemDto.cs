namespace BookProject.Dto
{
    public class GetOrderItemDto
    {
        public long BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public string Photo { get; set; }
    }
}
