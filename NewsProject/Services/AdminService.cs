using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsProject.Data;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Elfie.Model.Strings;
using Microsoft.EntityFrameworkCore;
using Azure.AI.Vision.ImageAnalysis;
using Azure;

namespace NewsProject.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        public readonly IHttpContextAccessor _httpContextAccessor;
        public readonly UserManager<User> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        
        [TempData]
        public string StatusMessage { get; set; }
        public AdminService(ApplicationDbContext context,
                    IHttpContextAccessor httpContextAccessor,
                    UserManager<User> userManager,
                    RoleManager<IdentityRole> roleManager,
                    IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        public void CreateArticle(Article article, User user, int categoryId, string tagnames)
        {
            // assigning 0 to views and likes for new article
            article.Views = 0;
            article.Likes = 0;

            // fetching category from category table
            // based on category name
            // and assigning it as foreign key for Article table
            var category = _context.Categories
                            .FirstOrDefault(C => C.Id == categoryId);
            article.Category = category;

            article.User = user;

            //adding article object to database
            if (user.UserRole == "Editor")
                article.IsApproved = true; // for editors
            _context.Articles.Add(article);
            _context.SaveChanges();

            // add UserID and Article ID(foreugn key)
            // in Author table
            Author author = new Author();
            author.UserID = user.Id;
            author.Article = article;
            if (user.UserRole == "Editor")
                author.Message = "Approved"; // for editors
            else
                author.Message = "Approval Pending"; // for other Authors

            _context.Authors.Add(author);
            _context.SaveChanges();

            // split the tagname list string 
            // which is separated by ','
            // add it into tag table
            var tagnamelist = tagnames.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tagname in tagnamelist)
            {
                Tag tag = new Tag();
                tag.Name = tagname.Trim().ToLower();
                tag.Article = article;
                _context.Tags.Add(tag);
                _context.SaveChanges();
            }
            var session = _httpContextAccessor.HttpContext.Session;
            session.Remove("CategoryList");
        }
        public void FetchCategories()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            List<CategoryNameVM> categories = new List<CategoryNameVM>();
            var categorylist = _context.Categories.ToList();

            foreach (var item in categorylist)
            {
                CategoryNameVM CN = new CategoryNameVM();
                CN.Name = item.Name.ToString();
                CN.CategoryId = item.Id; 
                categories.Add(CN);
            }
            session.SetString("CategoryList", JsonConvert.SerializeObject(categories));
        }

        // fetch roles from database to regiter an employee 
        public void FetchRolesStored()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var rolelist = _roleManager.Roles
                            .Where(r=>r.Name == "Admin" || r.Name == "Author" || r.Name == "Editor")
                            .Select(r => r.Name)
                            .ToList();
            session.SetString("RoleList", JsonConvert.SerializeObject(rolelist));
        }

        public List<Article> GetUnApprovedArticles()
        {
            var articles = _context.Articles
                            .Where(a => a.IsApproved == false && a.IsArchived != true)
                            .ToList();
            return articles;
        }
        public Article GetArticleById(int Id)
        {
            return _context.Articles.FirstOrDefault(a => a.Id == Id);
        }
        public void UpdateArticle(Article article)
        {
            _context.Update(article);
            _context.SaveChanges();
        }
        public string ApproveArticle(int articleID, string message)
        {
            string returnmessage;
            var articleInArticles = _context.Articles
                        .FirstOrDefault(a => a.Id == articleID);
            var articleInAuthors = _context.Authors
                        .FirstOrDefault(a => a.Article.Id == articleID);
            if (articleInArticles != null && articleInAuthors != null)
            {
                // update article table with IsApproved field
                articleInArticles.IsApproved = true;
                _context.Articles.Update(articleInArticles);
                _context.SaveChanges();

                // Update Author Table with message field
                if (message != null)
                    articleInAuthors.Message = message;
                else
                    articleInAuthors.Message = "Approved";

                _context.Authors.Update(articleInAuthors);
                _context.SaveChanges(true);

                returnmessage = "Approved";
            }
            else
            {
                returnmessage = "Error";
            }
            return returnmessage;
        }
        public string DeleteArticle(int articleID, string message)
        {
            string returnmessage;
            var articleInArticles = _context.Articles
                        .FirstOrDefault(a => a.Id == articleID);
            var articleInAuthors = _context.Authors
                        .FirstOrDefault(a => a.Id == articleID);
            if (articleInArticles != null && articleInAuthors != null)
            {
                // Article Table Updated
                _context.Articles.Remove(articleInArticles);
                _context.SaveChanges();

                // update author table with message
                if (message != null)
                    articleInAuthors.Message = message;
                else
                    articleInAuthors.Message = "Deleted";

                _context.Authors.Update(articleInAuthors);
                _context.SaveChanges();

                returnmessage = "Deleted";
            }
            else
            {
                returnmessage = "Error";
            }
            return returnmessage;
        }

        public string RejectArticle(int articleId, string message)
        {
            string returnmessage;
            var articleInArticles = _context.Articles
                        .FirstOrDefault(a => a.Id == articleId);
            var articleInAuthors = _context.Authors
                        .FirstOrDefault(a => a.Article.Id == articleId);
            if (articleInArticles != null && articleInAuthors != null)
            {
                // Article Table Updated
                articleInArticles.IsApproved = false;
                _context.Articles.Update(articleInArticles);
                _context.SaveChanges();

                // update author table with message
                if (message != null)
                    articleInAuthors.Message = message;
                else
                    articleInAuthors.Message = "Rejected";

                _context.Authors.Update(articleInAuthors);
                _context.SaveChanges();

                // no need to update/delete in Author Table 
                returnmessage = "Rejected";
            }
            else
            {
                returnmessage = "Error";
            }
            return returnmessage;
        }

        public List<string> GetAllCategoryList()
        {
            var categoryNameList= _context.Categories
                                .Select(c=>c.Name)
                                .ToList();
            return categoryNameList;
        }
        public void AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }
        public List<Article> SelectPreviousEditorsChoice()
        {
            var previousEditorChoiceList = _context.Articles
                           .Where(a => a.EditorChoice && a.IsArchived != true)
                           .ToList();
            return previousEditorChoiceList;
        }
        public List<Article> EditorsChoiceList()
        {
            var articleList = _context.Articles
                            .Where(a => a.IsArchived != true && a.IsApproved)
                            .ToList();
            return articleList;
        }

        public DetailNewsVM DetailNews(int id)
        {
            // this code is to count the view 
            var article = _context.Articles
                            .FirstOrDefault(a => a.Id == id);
            
            // Query for the latest news to display
            var newsResult = _context.Articles
                        .Where(a => a.Id == id)
                        .Include(c => c.Category)
                        .Include(u => u.User)
                        .Select(result => new DetailNewsVM()
                        {
                            ArticleId = result.Id,
                            DateStamp = result.DateStamp,
                            Likes = result.Likes,
                            LinkText = result.LinkText,
                            Headline = result.Headline,
                            CategoryName = result.Category.Name,
                            Content = result.Content,
                            Continent = result.Continent,
                            ImageLink = result.ImageLink,
                            ContentSummary = result.ContentSummary,
                            Views = result.Views,
                            AuthorName = result.User.FirstName + " " + result.User.LastName
                        }).FirstOrDefault();
            return newsResult;
        }
        public void SetEditorChoiceToFalse() // 
        {
            var articleList = _context.Articles
                            .Where(a => a.IsArchived != true && a.IsApproved)
                            .ToList();
            foreach (var article in articleList)
            {
                article.EditorChoice = false;
                _context.Articles.Update(article);
                _context.SaveChanges();
            }
        }
        public void AddArticleToEditorChoice(int articleID)
        {
            var article = _context.Articles.FirstOrDefault(a => a.Id == articleID);
            article.EditorChoice = true;
            _context.Articles.Update(article);
            _context.SaveChanges();
        }
        public void RemoveArticleFromEditorChoice(int articleID)
        {
            var article = _context.Articles.FirstOrDefault(a => a.Id == articleID);
            article.EditorChoice = false;
            _context.Articles.Update(article);
            _context.SaveChanges();
        }
        public List<ViewArticleByAuthorVM> ViewArticleByAuthor(string userId)
        {
            var articleList = _context.Authors
                                .Where(a => a.UserID == userId)
                                .Select(result => new ViewArticleByAuthorVM
                                {
                                    StatusMessage = result.Message,
                                    SingleArticle = result.Article
                                })
                               .ToList();
            return articleList;
        }
        public List<EmployeeListVM> GetEmployeesList()
        {
            var employees = _context.Users
               .Where(u => u.UserRole == "Author" || u.UserRole == "Editor" || u.UserRole == "InActive")
               .Select(result => new EmployeeListVM()
               {
                  
                   Fullname = result.FirstName + " " + result.LastName,
                   EmailAddress = result.UserName,
                   UserRole = result.UserRole,
                   Id = result.Id
               })
               .ToList();
            return employees;
        }

        public User GetEmployeeById(string id)
        {
            // Fetch user matching the ID and role
            var employee = _context.Users
                .Where(u => u.UserRole == "Author" || u.UserRole == "Editor" || u.UserRole == "InActive")
                .FirstOrDefault(u => u.Id == id);
            return employee;
        }

        public List<string> GetRoles()
        {
            // Fetch roles for dropdown
            var roles = _context.Roles
                    .Where(r => r.Name == "Author" || r.Name == "Editor" || r.Name == "InActive")
                    .Select(r => r.Name)
                    .ToList();
            return roles;
        }
        public async Task<string> UpdateEmployeeRole(User user, string rolename)
        {     
            // save the change in NetUsers table
            _context.Users.Update(user);
            _context.SaveChanges();
            return "Succeed";
        }
        public async Task<List<string>> ExtractAndStoreImageTags(Article article)
        {
            // 🔹 1. Get Azure Credentials
            string endpoint = _configuration["AzureCognitiveEndpoint"];
            string key = _configuration["AzureCognitiveKey"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Azure Computer Vision credentials are missing.");
            }

            // List to hold the tags associated with the article
            List<ImageTag> articleTags = new List<ImageTag>();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await article.File.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
                var imageData = BinaryData.FromStream(memoryStream);
                ImageAnalysisResult result = await client.AnalyzeAsync(imageData, VisualFeatures.Tags);

                // Extract and store top 5 detected tags
                List<string> detectedTags = result.Tags.Values.Select(t => t.Name).Take(5).ToList();

                // 🔹 6. Store Tags in Database
                foreach (var tag in detectedTags)
                {

                    var newTag = new ImageTag { Name = tag };
                    _context.ImageTags.Add(newTag);
                  // Add the tag to the article's tags list
                    articleTags.Add(newTag);

                }


                   

                // Add the tags to the article
                article.ImageTags = articleTags;


                // Save changes to the database (both tags and article)
                await _context.SaveChangesAsync();

                return detectedTags;
            }
        }

        public List<ImageTagsVM> ImageTagsList()
        {
            var imagesWithTags =  _context.Articles
                .Select(article => new ImageTagsVM
                {
                    ImageLink = article.ImageLink,
                    Tags = _context.ImageTags
                        .Where(tag => article.ImageTags.Contains(tag))
                        .Select(tag => tag.Name)
                        .ToList()
                })
                .ToList();

            return imagesWithTags;
        }


    }
}
