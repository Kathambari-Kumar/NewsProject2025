using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    // INSTEAD OF THIS VM DetailNews VM USED
    public class ArticleByCategory 
    {
        public int Id { get; set; }
        public string Headline { get; set; }= string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public DateTime DateStamp { get; set; }
        public string ContentSummary { get; set; }= string.Empty;
        public  string CategoryName { get; set; } = string.Empty ;
    }
}
