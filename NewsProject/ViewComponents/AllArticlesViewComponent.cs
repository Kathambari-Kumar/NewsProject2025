using Microsoft.AspNetCore.Mvc;
using NewsProject.Data;
using NewsProject.Models.VM;

namespace NewsProject.ViewComponents
{
    public class AllArticlesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AllArticlesViewComponent(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
            
        }
        public async Task<IViewComponentResult> InvokeAsync(int pageNumber = 1, int pageSize = 5)
        {
            var allArticles = _applicationDbContext.Articles
                              .Where(a => a.IsArchived == false && a.IsApproved)
                              .OrderBy(a => a.DateStamp) 
                              .Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToList();

            // Get total count for pagination controls
            var totalArticles = _applicationDbContext.Articles.Count(a => a.IsArchived == false && a.IsApproved);

            // Create a view model for articles and pagination data
            var viewModel = new PaginatedArticlesVM
            {
                Articles = allArticles,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalArticles / (double)pageSize)
            };
            return View(viewModel);
        }
    }
}
