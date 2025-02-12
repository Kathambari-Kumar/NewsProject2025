using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;

namespace NewsProject.ViewComponents
{
    public class SubscriptionTypeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _dbContext;
        public SubscriptionTypeViewComponent(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var suscriptiontypelist = await _dbContext.SubscriptionsTypes
                                .ToListAsync();
            return View(suscriptiontypelist);
        }
    }
}
