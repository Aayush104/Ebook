namespace BookProject.Dto
{
    public class GetReviewDto
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }

        public string Comment { get; set; }



        public DateTime CreatedTime
        {
            get; set;
        }
    }
}

