using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using NewsProject.Services;
using Azure.AI.Vision.ImageAnalysis;
using System;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

using System.Diagnostics;

namespace NewsProject.Controllers
{
    public class NewsArticleController : Controller
    {
        public readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;
        private readonly INewsArticleService _newsArticleService;


        // Get Azure Computer Vision credentials from environment variables
        private static readonly string endpoint = "https://compvisgr2408.cognitiveservices.azure.com/";
        private static readonly string  key = "5P8vjlQ5Pt9H3Vu6UYYnEB14FN8JrxkjKXX7QbyblKzksbv0XUY9JQQJ99BAACYeBjFXJ3w3AAAFACOGEJoD";
        public NewsArticleController(ILogger<HomeController> logger, 
            ApplicationDbContext dbContext,
            INewsArticleService newsArticleService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _newsArticleService = newsArticleService;

        }
        public IActionResult DetailNewsDisplay(int id) 
        {
            DetailNewsVM newsResult;
            if (User.IsInRole("Reader"))
                newsResult = _newsArticleService.DetailNewsDisplay(id, true);
            else
                newsResult = _newsArticleService.DetailNewsDisplay(id, false);

            return View(newsResult);
        }

        public IActionResult CategoryBasedArticles(string categoryname, int page = 1, int length = 4)
        {
            if (string.IsNullOrEmpty(categoryname))
            {
                return NotFound();
            }

            // Fetch paginated articles
            var articles = _newsArticleService.CategoryBasedArticles(categoryname, length, page);

            // Pass additional pagination details to the view
            var totalArticles = _newsArticleService.GetTotalArticlesByCategory(categoryname);
            var totalPages = (int)Math.Ceiling(totalArticles / (double)length);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CategoryName = categoryname;

            return View(articles);
        }
        public IActionResult CountLikes(int articleId)
        {
            int count = _newsArticleService.CountLikes(articleId);
            return Json(count);
        }

        public IActionResult WorldArticlesByContinent(string continentname, int page = 1, int length = 4) 
        {
            if (string.IsNullOrEmpty(continentname))
            {
                return BadRequest("Continent name is required."); // Ensure a continent is provided
            }
            else
            {
                WorldNewsVM articles = _newsArticleService.WorldArticlesByContinent(continentname, length, page);
                var totalArticles = _newsArticleService.GetTotalArticlesByContinent(continentname);
                var totalPages = (int)Math.Ceiling(totalArticles / (double)length);
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.ContinentName = continentname;
                return View(articles);
            }
        }

        // code to search bar (tag based search)
        //[HttpGet]
        //public IActionResult OnGetSearchByTag()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult OnPushSearchByTag(string word)
        //{
        //    //string word = Request.Form["searchword"];
        //    word = word.Substring(0, word.Length - 1);
        //    word = word.Trim().ToLower();
        //    List<BriefNewsVM> articleList = _newsArticleService.OnPushSearchByTag(word);
        //    if (articleList.Count == 0)
        //        TempData["SearchStatus"] = word;

        //    return View(articleList);
        //}

        [HttpPost]
        public IActionResult SearchArticleByTag()
        {
            string word = Request.Form["searchword"];
            //word = word.Substring(0, word.Length - 1);
            word = word.Trim().ToLower();
            List<BriefNewsVM> articleList = _newsArticleService.OnPushSearchByTag(word);
            if (articleList.Count == 0)
                TempData["SearchStatus"] = word;

            return View(articleList);
        }

        public IActionResult PodcastNews()
        {
            return View();
        }

        public IActionResult GetVoiceLink(int Id)
        {
            var voiceUrl = _newsArticleService.GetVoiceUrlLink(Id);
            return Json(voiceUrl);
        }
        public IActionResult WeatherAndTechVC(int count)
        {
            return ViewComponent("WeatherAndTech", new { count = count });
        }

        public IActionResult SportsVC(int count)
        {
            return ViewComponent("Sports", new { count = count });
        }
        public IActionResult AllArticlesVM(int pageNumber, int pageSize = 8)
        {
            return ViewComponent("AllArticles", new { pageNumber = pageNumber, pageSize = pageSize });
        }

        [HttpPost]
        public async Task<IActionResult> Translate(int id, string lang)
        {
            var translatedArticle = await _newsArticleService.TranslateArticleAsync(id, lang);
            if (translatedArticle == null)
                return Json(new { success = false, message = "error" });
            return Json(new
            {
                success = true,
                translatedLinkText = translatedArticle.TranslatedLinkText,
                translatedContent = translatedArticle.TranslatedContent,
                translatedContentSummary = translatedArticle.TranslatedContentSummary
            });           
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();  // This returns the upload form
        }

        [HttpPost]
        public async Task<IActionResult> Upload(Article model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ViewBag.Message = "Please select an image to upload.";
                return View();
            }

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                return BadRequest("Azure Computer Vision credentials are missing.");
            }

            // Read the uploaded image into a stream
            using MemoryStream memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);
            memoryStream.Position = 0;


            // Create the Computer Vision client
            var client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));


            // Analyze the image for tags
            var imageData = BinaryData.FromStream(memoryStream);
            ImageAnalysisResult result = await client.AnalyzeAsync(imageData, VisualFeatures.Tags);


            // Extract detected tags
            List<string> detectedTags = result.Tags.Values.Select(t => t.Name).ToList();

            // Ensure ViewBag.Tags is always set to prevent null exceptions
            ViewBag.Tags = detectedTags.Any() ? detectedTags : new List<string>();
            ViewBag.Message = detectedTags.Any() ? "Tags extracted successfully." : "No tags detected.";




            return View();  // This returns the upload form
        }

      





       


    }
}
