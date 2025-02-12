using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.VM;

namespace NewsProject.ViewComponents
{
    public class MostPopularNewsViewComponent : ViewComponent
    {

        public readonly ApplicationDbContext _applicationDbContext;
        public MostPopularNewsViewComponent(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;           
        }

        //Popular news in each category
        public async Task<IViewComponentResult> InvokeAsync()
        {           
            var mostPopularResult = await _applicationDbContext.Categories
                .GroupJoin(_applicationDbContext.Articles
                .Where(a => a.IsApproved && a.IsArchived != true),
                c => c.Id,
                a => a.Category.Id,
                (c, a) => new BriefNewsVM()
                {
                    CategoryName = c.Name,
                    ArticleList = a.OrderByDescending(a => a.Views)
                                        .Take(5)
                                        .ToList()
                }).ToListAsync();
            return View(mostPopularResult);

        }
    }
}
