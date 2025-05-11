namespace BookProject.Dto
{
    public class MailDto
    {
        public string ToEmail { get; set; }
        public string FullName { get; set; }
        public string ClaimCode { get; set; }
        public DateTime OrderDate { get; set; }
        public int TotalBooks { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? Discount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
