namespace BookProject.Dto
{
    public class BookFilterParams
    {
        public string? Search { get; set; }
        public string? Genre { get; set; }
        public string? Author { get; set; }
        public string? Publisher { get; set; }
        public string? Language { get; set; }
        public string? Format { get; set; }
        public bool? InStock { get; set; }
        public bool? InLibrary { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
