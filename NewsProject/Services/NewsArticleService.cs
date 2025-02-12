 
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.API;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using System.Security.Policy;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace NewsProject.Services
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient1;
        private readonly HttpClient _httpClient2;
        private readonly HttpClient _httpClient3;
        private readonly HttpClient _httpClient4;
        private readonly ApplicationDbContext _dbContext;

       
        private readonly string _subscriptionKey = "43m0fQnXfpK1VAl1Mwdafi7AQWD7byGezVh9cn9fiB2Ux2viD6qmJQQJ99BBACfhMk5XJ3w3AAAbACOG36ni";
        private readonly string _endpoint = "https://api.cognitive.microsofttranslator.com";
        private readonly string _location = "swedencentral";


        public NewsArticleService(IHttpClientFactory httpClientFactory,
            ApplicationDbContext dbContext)
        {
            _httpClient = httpClientFactory.CreateClient("forecast");
            _httpClient1 = httpClientFactory.CreateClient("v4/weather/realtime");
            _httpClient2 = httpClientFactory.CreateClient("v6/latest/USD");
            _httpClient3 = httpClientFactory.CreateClient("CoinGeckoAPI");
            _httpClient4 = httpClientFactory.CreateClient("CryptoHistoricData");
            _dbContext = dbContext;
        }

        // weather forecast from Lexicon
        public async Task<WeatherForecast> GetForecast(string chosenCity)
        {
            var forecastResponse = await _httpClient.GetStringAsync($"forecast?city={chosenCity}&lang=en");
            var response = JsonConvert.DeserializeObject<WeatherForecast>(forecastResponse) ??
                new WeatherForecast();
            return response;
        }

        // Exchange Rate API
        public async Task<ExchangeRates> GetExchangeRate()
        {
            var exchangeRate = await _httpClient2.GetStringAsync("v6/latest/USD");
            var exchangeRateResponse = JsonConvert.DeserializeObject<ExchangeRates>(exchangeRate) ??
                new ExchangeRates();
            return exchangeRateResponse;
        }
       
        public DetailNewsVM DetailNewsDisplay(int id, bool IsReader)
        {
            // this code is to count the view 
            var article = _dbContext.Articles
                            .FirstOrDefault(a => a.Id == id);
            // ViewCount will be increased only if the User is reader 
            if (article != null || IsReader == true)
            {
                var count = article.Views;
                count = count + 1;
                article.Views = count;
                _dbContext.Articles.Update(article);
                _dbContext.SaveChanges();
            }
            // Query for the latest news to display
            var newsResult = _dbContext.Articles
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
                            AuthorName = result.User.FirstName + " " + result.User.LastName,
                            AudioUrl = result.VoiceInput
                        }).FirstOrDefault();
            return newsResult;
        }

        public List<BriefNewsVM> CategoryBasedArticles(string categoryname, int length, int page)
        {
            int skip = (page - 1) * length;
            var articles = _dbContext.Categories
                            .Where(c => c.Name.Equals(categoryname))
                            .GroupJoin(_dbContext.Articles
                                    .Where(a=>a.IsApproved && a.IsArchived != true),
                                    c => c.Id,
                                    a => a.Category.Id,
                                    (c, a) => new BriefNewsVM
                                    {
                                        CategoryName = c.Name,
                                        ArticleList = a.OrderByDescending(article => article.DateStamp)
                                                       .Skip(skip)
                                                       .Take(length)
                                                       .ToList()
                                    })
                                .ToList();
            return articles;
        }
        public int GetTotalArticlesByCategory(string categoryname)
        {
            return _dbContext.Articles
                .Where(a=>a.IsApproved && a.IsArchived != true)
                .Count(a => a.Category.Name.ToLower() == categoryname.ToLower());
        }
        public int CountLikes(int articleId)
        {
            var article = _dbContext.Articles
                            .FirstOrDefault(a => a.Id == articleId);
            var count = article.Likes;
            count = count + 1;
            article.Likes = count;
            _dbContext.Articles.Update(article);
            _dbContext.SaveChanges();
            return count;
        }
        public WorldNewsVM WorldArticlesByContinent(string continentname, int length, int page)
        {
            int skip = (page - 1) * length;
            var articles = _dbContext.Articles
                         .Where(a => a.IsApproved && a.IsArchived != true
                                        && a.Continent == continentname)
                         .Include(a => a.Category)
                         .OrderByDescending(a => a.DateStamp)
                         .ToList();
            var viewModel = new WorldNewsVM
            {
                Continent = continentname,
                articles = articles.Skip(skip).Take(length).ToList()
            };
            return viewModel;
        }

        public int GetTotalArticlesByContinent(string continentName)
        {
            return _dbContext.Articles
                .Where(a => a.IsApproved && a.IsArchived != true)
                .Count(a => a.Continent.ToLower() == continentName.ToLower());
        }
        //public List<BriefNewsVM> OnPushSearchByTag(string searchWord)
        // {
        //    var articlelist = _dbContext.Tags
        //            .Where(t => t.Name == searchWord)
        //            .GroupJoin(_dbContext.Articles.Where(a => a.IsApproved),
        //            t => t.Article.Id,
        //            a => a.Id,
        //            (t, a) => new BriefNewsVM
        //            {
        //                CategoryName = t.Name,
        //                ArticleList = a.ToList(),
        //            })
        //            .ToList();
        //    return articlelist;
        //}

        public List<BriefNewsVM> OnPushSearchByTag(string searchWord)
        {
            var manualTagsQuery = _dbContext.Tags
                .Where(t => t.Name == searchWord)
                .SelectMany(t => _dbContext.Articles.Where(a => a.IsApproved && a.Id == t.Article.Id))
                .Select(a => new { CategoryName = searchWord, Article = a })
                .ToList();

            var imageTagsQuery = _dbContext.ImageTags
                .Where(it => it.Name == searchWord)
                .SelectMany(it => _dbContext.Articles.Where(a => a.IsApproved && a.Id == it.Article.Id))
                .Select(a => new { CategoryName = searchWord, Article = a })
                .ToList();

            // Combine the results and group in memory
            var combinedResults = manualTagsQuery
                .Concat(imageTagsQuery)
                .GroupBy(x => x.CategoryName)
                .Select(g => new BriefNewsVM
                {
                    CategoryName = g.Key,
                    ArticleList = g.Select(x => x.Article).Distinct().ToList() 
                })
                .ToList();

            return combinedResults;
        }


        public List<Article> GetPaginatedArticles(int length, int page)
        {
            int skip = (page - 1) * length;

            var articlesList = _dbContext.Articles
                               .Where(a=>a.IsApproved && a.IsArchived != true)
                               .OrderBy(a => a.Id)
                               .Skip(skip)
                               .Take(length)
                               .ToList();
            return articlesList;
        }
        public string GetVoiceUrlLink(int articleId)
        {
            var voiceUrl = _dbContext.Articles
                            .Where(a => a.Id == articleId)
                            .Select(a=>a.VoiceInput)
                            .ToList();
            return voiceUrl[0].ToString();
        }
        public async Task<Article> GetArticleByIdAsync(int id)
        {
            return await _dbContext.Articles
                  .Include(a => a.User) 
                  .Include(a => a.Category)
                  .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<DetailNewsVM> TranslateArticleAsync(int id, string targetLanguage)
        {
            var article = await GetArticleByIdAsync(id);
            if (article == null) return null;

            var translatedHeadline = await TranslateText(article.Headline, targetLanguage);
            var translatedContentSummary = await TranslateText(article.ContentSummary, targetLanguage);
            var translatedContent = await TranslateText(article.Content, targetLanguage);
            var translatedLinkText = await TranslateText(article.LinkText, targetLanguage);

            return new DetailNewsVM
            {
                ArticleId = article.Id,
                DateStamp = article.DateStamp,
                LinkText = article.LinkText,
                Views = article.Views,
                Likes = article.Likes,
                ImageLink = article.ImageLink,
                AuthorName = article.User?.FirstName + " " + article.User?.LastName,
                CategoryName = article.Category.Name,
                Headline = article.Headline,
                ContentSummary = article.ContentSummary,
                Content = article.Content,
                TranslatedLinkText = translatedLinkText,
                TranslatedHeadline = translatedHeadline,
                TranslatedContentSummary = translatedContentSummary,
                TranslatedContent = translatedContent,
                Language = targetLanguage
            };
        }
        private async Task<string> TranslateText(string text, string targetLanguage)
        {
            string route = $"/translate?api-version=3.0&to={targetLanguage}";
            object[] body = new object[] { new { Text = text } };
            string requestBody = JsonConvert.SerializeObject(body);

            using var client = new HttpClient();
            using var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_endpoint + route),
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

            request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

            HttpResponseMessage response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            dynamic jsonResponse = JsonConvert.DeserializeObject(result);
            return jsonResponse[0].translations[0].text;
        }
    }
}
