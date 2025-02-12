using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    public class PaginatedArticlesVM
    {
        public List<Article> Articles { get; set; } // List of articles for the current page
        public int CurrentPage { get; set; }       // Current page number
        public int TotalPages { get; set; }        // Total number of pages
    }
}
