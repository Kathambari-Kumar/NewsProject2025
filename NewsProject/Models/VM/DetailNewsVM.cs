namespace NewsProject.Models.VM
{
    public class DetailNewsVM
    {
        public int ArticleId { get; set; }
        public DateTime DateStamp { get; set; } = DateTime.Now;
        public required string LinkText { get; set; }
        public required string Headline { get; set; }
        public required string ContentSummary { get; set; }
        public required string Content { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public required string ImageLink { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Continent { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;

        public string AudioUrl { get; set; } = string.Empty;

        public string TranslatedLinkText { get; set; } = string.Empty;
        public string TranslatedHeadline { get; set; } = string.Empty;
        public string TranslatedContentSummary { get; set; } = string.Empty;
        public string TranslatedContent { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
