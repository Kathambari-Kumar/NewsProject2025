using NewsProject.Models.DB;
using NewsProject.Models.VM;
using SixLabors.ImageSharp.Formats;

namespace NewsProject.Services
{
    public interface IAdminService
    {
        public void CreateArticle(Article article, User user, int categoryId, string tagnames);
        public void FetchCategories();
        public void FetchRolesStored();
        public List<Article> GetUnApprovedArticles();
        public Article GetArticleById(int Id);
        public void UpdateArticle(Article article);
        public string ApproveArticle(int articleId, string message);
        public string DeleteArticle(int articleId, string message);
        public string RejectArticle(int articleId, string message);
        public void AddCategory(Category category);
        public List<string> GetAllCategoryList();
        public List<Article> EditorsChoiceList();
        public DetailNewsVM DetailNews(int id);
        public void AddArticleToEditorChoice(int articleID);
        public void RemoveArticleFromEditorChoice(int articleID);
        public void SetEditorChoiceToFalse();
        public List<Article> SelectPreviousEditorsChoice();
        public List<ViewArticleByAuthorVM> ViewArticleByAuthor(string userId);
        public List<EmployeeListVM> GetEmployeesList();
        public User GetEmployeeById(string id);
        public List<string> GetRoles();
        public Task<string> UpdateEmployeeRole(User user, string rolename);
        Task<List<string>> ExtractAndStoreImageTags(Article article);
        public List<ImageTagsVM> ImageTagsList();

    }
}
