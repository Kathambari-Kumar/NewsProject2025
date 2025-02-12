using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    public class ViewArticleByAuthorVM
    {
        public string StatusMessage { get; set; } = string.Empty;
        public Article SingleArticle { get; set; }
    }
}
