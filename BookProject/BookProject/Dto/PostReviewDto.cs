namespace BookProject.Dto
{
    public class PostReviewDto
    {
        public long BookId { get; set; }

        public int? Star { get; set; }
        public string Comment { get; set; }
    }
}
