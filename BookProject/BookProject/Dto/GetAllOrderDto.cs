namespace BookProject.Dto
{
    public class GetAllOrderDto
    {
        public long OrderId { get; set; }
        public string ClaimCode { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountApplied { get; set; }
        public List<GetOrderItemDto> OrderItems { get; set; }
    }
}
