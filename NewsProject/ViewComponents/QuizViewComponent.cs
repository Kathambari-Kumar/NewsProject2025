using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.VM;

namespace NewsProject.ViewComponents
{
    public class QuizViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public QuizViewComponent(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var quiz = await _applicationDbContext.Quizzes
                        .Select(q => new QuizVM
                        {
                            Id = q.Id,
                            ImageLink = q.ImageLink,
                            Description = q.Description
                        }).FirstOrDefaultAsync();

            return View(quiz);
        }
    }
}
