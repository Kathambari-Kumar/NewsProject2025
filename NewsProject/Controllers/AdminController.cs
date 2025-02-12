using NewsProject.Models.DB;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using NewsProject.Services;
using Azure.Storage.Blobs;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using NewsProject.Extensions;
using Org.BouncyCastle.Utilities.Zlib;
using NewsProject.Models.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.AspNetCore.Http.HttpResults;
using Humanizer;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.EntityFrameworkCore;
using Azure;
using NewsProject.Data;

namespace NewsProject.Controllers
{
    [Authorize(Roles = "Admin, Editor, Author")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
       
        [TempData]
        public string StatusMessage { get; set; }
        public AdminController(IAdminService adminService,
            UserManager<User> userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _adminService = adminService;
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        [HttpGet]
        public IActionResult OnGetCreatingArticle()
        {
            return View();
        }
        [HttpPost, ActionName("OnGetCreatingArticle")]
        public async Task<IActionResult> OnPostCreatingArticle(Article article)
        {
            //1)  Retrieve category ID & tags from form

            int categoryId = Convert.ToInt32(Request.Form["categories"]);
            string tagnames = Request.Form["tagnames"];

            //2.Fetch current user
             
            var user = _userManager.GetUserAsync(User).Result;
            if (User.IsInRole("Editor"))
            {
                article.IsApproved = true;
            }
            article.DateStamp = DateTime.Now;

            // Blob Storage Process
            //3. Get Azure Blob Storage configuration

            var thumbnailWidth = 100;
            string imgurl = string.Empty;
            string audiourl = string.Empty;
            string connectionString = _configuration["AzureBlobConnectionString"];
            string containerName = _configuration["AzureBlobContainerName"];
            string audiocontainerName = _configuration["AzureAudioBlobContainerName"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobContainerClient audioContainerClient = blobServiceClient.GetBlobContainerClient(audiocontainerName);
            containerClient.CreateIfNotExists();
            audioContainerClient.CreateIfNotExists();

            // 4. Upload and process image
            
            BlobClient blobClient = containerClient.GetBlobClient(article.File.FileName);
            Stream outputStream = new MemoryStream();
            using (var stream = article.File.OpenReadStream())
            {
                using (Image image = Image.Load(stream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Stretch,
                        Size = new Size(800, 350)
                    }));
                    var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                    image.Save(outputStream, jpegEncoder);
                    outputStream.Position = 0;
                    blobClient.Upload(outputStream, true);
                    stream.Dispose();
                    imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                }
            }    
            article.ImageLink = imgurl; // store the URL as ImageLink

            //  5. Extract Tags Using Azure Computer Vision

            await _adminService.ExtractAndStoreImageTags(article);
  
            // 6.Convert Article Text to Audio(Text-to-Speech)

            var fileName = $"{Guid.NewGuid()}.mp3";
            BlobClient audioBlobClient = audioContainerClient.GetBlobClient(fileName);
            
            var speechKey = "DLlfXEDJh9HqyS1Xl9oGJfHUwUCd5cuS43lRp4rMDfeXdlssu5P5JQQJ99BAACfhMk5XJ3w3AAAYACOGcWKZ";
            var speechRegion = "swedencentral"; 
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

            //      SYNTHESIZE SPEECH TO A SPECIFIC FILE

            //var audioFilePath = $"C:/Users/Student/Music/{fileName}";
            //using var audioConfig = AudioConfig.FromWavFileOutput(audioFilePath);
            //var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

            //      SYNTHESIZE SPEECH TO CURRENT ACTIVE OUTPUT
            //var speechSynthesizer = new SpeechSynthesizer(speechConfig);

            //      Get a result as an in-memory stream
            var speechSynthesizer = new SpeechSynthesizer(speechConfig, null);
            string textInput = article.ContentSummary + article.Content;
            var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(textInput);           
            if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    var audioStream = new MemoryStream(speechSynthesisResult.AudioData);  
                    audioBlobClient.Upload(audioStream, true);
                }           
            article.VoiceInput = audioBlobClient.Uri.AbsoluteUri;

            //7. Save article to database

            _adminService.CreateArticle(article, user, categoryId, tagnames);
            TempData["Result"] = "The article record is added successfully!!";
            return RedirectToAction("AdminEditorAuthorFrontPage");
        }

        [HttpGet]
        public IActionResult DisplayImageTags()
        {
            var imagesWithTags = _adminService.ImageTagsList();
            return View(imagesWithTags);
        }
        public IActionResult FetchCategoriesList()
        {
            _adminService.FetchCategories();
            return RedirectToAction("OnGetCreatingArticle");
        }

        // code to approve author's article
        public IActionResult GetUnApprovedArticles()
        {
            // get all the articles which are not approved
            var articles = _adminService.GetUnApprovedArticles();
            return View(articles);
        }
        public IActionResult FetchRolesStored()
        {
            _adminService.FetchRolesStored();
            return RedirectToPage("/Account/Register", new { area = "Identity" });
        }
        public IActionResult UpdateArticleByAuthor(int id)
        {
            var article = _adminService.GetArticleById(id);
            return View(article);
        }

        [HttpPost]
        public IActionResult UpdateArticleByAuthor(Article article)
        {
            if (article.File != null)
            {
                string imgurl = string.Empty;
                string connectionString = _configuration["AzureBlobConnectionString"];
                string containerName = _configuration["AzureBlobContainerName"];
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                containerClient.CreateIfNotExists();

                BlobClient blobClient = containerClient.GetBlobClient(article.File.FileName);

                Stream outputStream = new MemoryStream();
                using (var stream = article.File.OpenReadStream())
                {
                    using (Image image = Image.Load(stream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Stretch,
                            Size = new Size(800, 350)
                        }));
                        var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                        image.Save(outputStream, jpegEncoder);
                        outputStream.Position = 0;
                        blobClient.Upload(outputStream, true);
                        stream.Dispose();
                        imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                    }
                }
                article.ImageLink = imgurl; // store the URL as ImageLink
            }
            _adminService.UpdateArticle(article);
            TempData["Result"] = "The article record is updated successfully!!";
            return RedirectToAction("ViewArticleByAuthor");
        }
       
        [HttpGet]
        public IActionResult UpdateArticleByEditor(int id)
        {
            var article = _adminService.GetArticleById(id);
            return View(article);
        }
        [HttpPost]
        public IActionResult UpdateArticleByEditor(Article article)
        {
            if (article.File != null)
            {
                string imgurl = string.Empty;
                string connectionString = _configuration["AzureBlobConnectionString"];
                string containerName = _configuration["AzureBlobContainerName"];
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                containerClient.CreateIfNotExists();

                BlobClient blobClient = containerClient.GetBlobClient(article.File.FileName);

                Stream outputStream = new MemoryStream();
                using (var stream = article.File.OpenReadStream())
                {
                    using (Image image = Image.Load(stream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Stretch,
                            Size = new Size(800, 350)
                        }));
                        var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                        image.Save(outputStream, jpegEncoder);
                        outputStream.Position = 0;
                        blobClient.Upload(outputStream, true);
                        stream.Dispose();
                        imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                    }
                }
                article.ImageLink = imgurl; // store the URL as ImageLink
            }
            _adminService.UpdateArticle(article);
            TempData["Result"] = "The article record is updated successfully!!";
            return RedirectToAction("GetUnApprovedArticles");
        }
        public IActionResult ApproveArticle(int articleID, string message)
        {
            string returnmessage = _adminService.ApproveArticle(articleID, message);
            return Json(returnmessage);
        }

        public IActionResult DeleteArticle(int articleID, string message)
        {
            var returnmessage = _adminService.DeleteArticle(articleID, message);
            return Json(returnmessage);
        }

        [HttpPost]
        public IActionResult RejectArticle(int articleId, string message)
        {
            var returnmessage = _adminService.RejectArticle(articleId, message);
            return Json(returnmessage);
        }
        // Adding Roles to Database
        [HttpGet]
        public async Task<IActionResult> OnGetAddRole()
        {
            return View();
        }
        [HttpPost, ActionName("OnGetAddRole")]
        public async Task<IActionResult> OnPostAddRole()
        {
            var role = Request.Form["rolename"];
            //var addRoleResult = new IdentityRole(role).ToString();
            // creating a new role in database
            // table AspNetRoles
            //var result = await _roleManager.CreateAsync(new IdentityRole(role));
            return View();
        }

        public IActionResult AddCategory()
        {
            if (_httpContextAccessor.HttpContext.Session.GetString("CategoryNameList") != null )
                _httpContextAccessor.HttpContext.Session.Remove("CategoryNameList");

            List<string> categoryNameList = _adminService.GetAllCategoryList();
            _httpContextAccessor.HttpContext.Session.SetString("CategoryNameList", JsonConvert.SerializeObject(categoryNameList));
            return View();
        }

        [HttpPost]
        public IActionResult AddCategory(Category category)
        {

            if (category == null)
            {
                ModelState.AddModelError("", "Category type details are invalid.");
                return View(category); // Return to the same form with error messages.
            }
            _adminService.AddCategory(category);
            TempData["Result"] = "A new category record is successfully added to database!!";
            return RedirectToAction("AdminEditorAuthorFrontPage");
        }
        public IActionResult EditorsChoiceList()
        {
            var previousEditorChoiceList = _adminService.SelectPreviousEditorsChoice();
            List<int> count = new List<int>(); 
            foreach(var item in previousEditorChoiceList)
            {
                count.Add(item.Id);
            }
            HttpContext.Session.Set<List<int>>("EditorChoiceMaxLimit", count);
            var allArticleList = _adminService.EditorsChoiceList();
            // before selection all article's editor choice is set to false
            return View(allArticleList);
        }

        [HttpPost]
        public IActionResult SetEditorChoiceToFalse()
        {
            HttpContext.Session.Remove("EditorChoiceMaxLimit");
            _adminService.SetEditorChoiceToFalse();
            return Json("true");
        }

        public IActionResult DetailNewsForEditors(int id)
        {
            DetailNewsVM newsResult = _adminService.DetailNews(id);
            return View(newsResult);
        }
        public IActionResult SelectEditorChoiceArticle(int articleId)
        {
            var articleList = HttpContext.Session.Get<List<int>>("EditorChoiceMaxLimit") ?? new List<int> { };
            // checking the article selected already, it means unchecked the checkbox,
            if (articleList.Contains(articleId))
            {
                articleList.Remove(articleId);
                HttpContext.Session.Set<List<int>>("EditorChoiceMaxLimit", articleList);
                _adminService.RemoveArticleFromEditorChoice(articleId);
                return Json("deselected");
            }
            else
            {
                articleList.Add(articleId);
                var numberOfListItems = articleList.Count();
                if (numberOfListItems <= 3)
                {
                    HttpContext.Session.Set<List<int>>("EditorChoiceMaxLimit", articleList);
                    _adminService.AddArticleToEditorChoice(articleId);
                    return Json("success");
                }
                else
                {
                    return Json("danger");
                }
            }
        }
        public IActionResult ViewArticleByAuthor()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var articalList = _adminService.ViewArticleByAuthor(user.Id);
            return View(articalList);
        }
        public IActionResult GetEmployeesList()
        {
            var employeesList = _adminService.GetEmployeesList();
            return View(employeesList);
        }

        public IActionResult EditEmployee(string id)
        {
            var user = _adminService.GetEmployeeById(id);
            if (user == null) return NotFound();

            var model = new EmployeeEditVM
            {
                Id = user.Id,
                EmailAddress = user.Email,
                UserRole = user.UserRole,
                FullName = user.FirstName + " " + user.LastName,
                Roles = _adminService.GetRoles() // Populate dropdown
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployee(EmployeeEditVM model)
        {
            // NetUser table which comes with Identity 
            var identityUser =_userManager.FindByIdAsync(model.Id).Result;

            // model 'User' which we created
            var userInstance = _adminService.GetEmployeeById(model.Id);

            // remove the role already the employee had, and assign them to
            // a new role [deals with UserRole table ]
            var result = await _userManager.RemoveFromRoleAsync(identityUser, userInstance.UserRole);
            await _userManager.AddToRoleAsync(identityUser, model.UserRole);

            // Update the employee role in NetUser table
            userInstance.UserRole = model.UserRole;
            await _adminService.UpdateEmployeeRole(userInstance, model.UserRole);
            TempData["Result"] = $"Employee {model.FullName} 's role has been changed successfully!";
            return RedirectToAction("GetEmployeesList");
        }

        [HttpGet]

        public IActionResult CreateAudioFromText()
        {
            return View();
        }
        public async Task<IActionResult> CreateAudioFromText(string content)
        {
            string connectionString = _configuration["AzureBlobConnectionString"];
            string containerName = _configuration["AzureAudioBlobContainerName"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();

            content = Request.Form["contentText"];
            //content = "Hello, Good Morning";
            var fileName = $"{Guid.NewGuid()}.mp3";
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            //var audioFilePath = $"C:/Users/Student/Music/{fileName}";
            var speechKey = "DLlfXEDJh9HqyS1Xl9oGJfHUwUCd5cuS43lRp4rMDfeXdlssu5P5JQQJ99BAACfhMk5XJ3w3AAAYACOGcWKZ";
            var speechRegion = "swedencentral";

            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            //speechConfig.EndpointId = "https://swedencentral.api.cognitive.microsoft.com/";
            //speechConfig.SpeechSynthesisLanguage = "en-US";
            //speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
            //using var audioConfig = AudioConfig.FromWavFileOutput(audioFilePath);
            var speechSynthesizer = new SpeechSynthesizer(speechConfig, null);
            var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(content);
            var stream = AudioDataStream.FromResult(speechSynthesisResult);
            if (speechSynthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                var audioStream = new MemoryStream(speechSynthesisResult.AudioData);
                blobClient.Upload(audioStream, true);
            }
            var url = blobClient.Uri.AbsoluteUri;
            return Json(url);
        }
        public IActionResult AdminEditorAuthorFrontPage()
        {
            return View();
        }
    }
}

/// Author-1
/// lilliflower@gmail.com : Mango1#
/// Author-2
/// david345@hotmail.com, Mango1#
/// Admin -
/// annanilsson@gmail.com, Mango1#

