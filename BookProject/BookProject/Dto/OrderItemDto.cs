namespace BookProject.Dto
{
    public class OrderItemDto
    {
        public long BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
