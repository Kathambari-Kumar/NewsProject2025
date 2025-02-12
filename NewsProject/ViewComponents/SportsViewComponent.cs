using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.VM;

namespace NewsProject.ViewComponents
{
    public class SportsViewComponent : ViewComponent
    {
        public readonly ApplicationDbContext _dbContext;
        public SportsViewComponent(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int count) // if need, can pass parameters
        {
            var Articles = await _dbContext.Categories
                           .Where(a => a.Name.Equals("Sports"))
                           .GroupJoin(_dbContext.Articles
                           .Where(a => a.IsApproved && a.IsArchived != true),
                            c => c.Id,
                            a => a.Category.Id,
                            (c, a) => new BriefNewsVM()
                            {
                                CategoryName = c.Name,
                                ArticleList = a.OrderByDescending(a => a.DateStamp)
                                         .Take(count)
                                         .ToList()
                            })
                          .ToListAsync();
            return View(Articles);
        }
    }
}
