 using NewsProject.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using System.Text.RegularExpressions;
using NewsProject.Models.VM;
     
namespace NewsProject.ViewComponents
{
    public class LatestNewsViewComponent : ViewComponent
    {
        public readonly ApplicationDbContext _applicationDbContext;
        public LatestNewsViewComponent(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync() //InvokeAsync method queries the database for the latest news and sends it to the View Component's view.
        {
            var latestNewsResult = await _applicationDbContext.Categories
                .GroupJoin(_applicationDbContext.Articles
                .Where(a=>a.IsApproved && a.IsArchived != true),
                c => c.Id,
                a => a.Category.Id,
                (c, a) => new BriefNewsVM()
                {
                    CategoryName = c.Name,
                    ArticleList = a.OrderByDescending(a => a.DateStamp)
                                        .Take(2)
                                        .ToList()
                }).ToListAsync();
            return View(latestNewsResult);
            //return View(latestNews);
        }
    }
}
