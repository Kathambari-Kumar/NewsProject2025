
using Microsoft.AspNetCore.Mvc;
using NewsProject.Models.API;
using NewsProject.Models.DB;
using NewsProject.Models.VM;

namespace NewsProject.Services
{
    public interface INewsArticleService
    {
        public Task<WeatherForecast> GetForecast(string chosenCity);
       
        public Task<ExchangeRates> GetExchangeRate();
        public DetailNewsVM DetailNewsDisplay(int id, bool IsReader);
      

        public List<BriefNewsVM> CategoryBasedArticles(string categoryname, int length, int page);
        public int GetTotalArticlesByCategory(string categoryname);
        public int CountLikes(int articleId);
        public WorldNewsVM WorldArticlesByContinent(string continentname, int length, int page);
        public int GetTotalArticlesByContinent(string continentName);
        public List<BriefNewsVM> OnPushSearchByTag(string searchWord);

        
        public string GetVoiceUrlLink(int articleId);
        public List<Article> GetPaginatedArticles(int length, int page);


        
        Task<Article> GetArticleByIdAsync(int id);
        Task<DetailNewsVM> TranslateArticleAsync(int id, string targetLanguage);
    }
}
