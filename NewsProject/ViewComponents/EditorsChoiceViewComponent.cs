using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using Newtonsoft.Json;

namespace NewsProject.ViewComponents
{
    public class EditorsChoiceViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EditorsChoiceViewComponent(ApplicationDbContext applicationDbContext, 
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationDbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var articleList = await _applicationDbContext.Articles
                               .Where(a => a.EditorChoice && a.IsArchived == false)
                               .OrderByDescending(a => a.DateStamp)
                               .ToListAsync();
            if (articleList == null)
            {
                List<Article> articleListFromSession = JsonConvert.DeserializeObject<List<Article>>(session.GetString("PreviousEditorChoice"));
                return View(articleListFromSession);
            }
            return View(articleList);
        }
    }
}
