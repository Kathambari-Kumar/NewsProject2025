using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    public class BriefNewsVM
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<Article> ArticleList { get; set; } = new List<Article>();
    }
}
