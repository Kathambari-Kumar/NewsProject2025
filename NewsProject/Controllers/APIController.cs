using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using NewsProject.Models.API;
using NewsProject.Models.VM;
using NewsProject.Services;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using static System.Net.WebRequestMethods;

namespace NewsProject.Controllers
{
     [Route("API/[controller]")]
     [ApiController]
    public class APIController : Controller
    {
       private readonly IWeatherService _weatherService;
       private readonly IElectrictyService _electrictyService;
       private readonly HttpClient _httpClient;
      


        public APIController(IWeatherService weatherService, IElectrictyService electrictyService, HttpClient httpClient)
        {
            _weatherService = weatherService;
            _electrictyService = electrictyService;
            _httpClient = httpClient;
           


        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }


         [HttpGet("GetElectricityData")]
        
        public async Task<IActionResult> GetElectricityData(string date)
        {
            var electricityData = await _electrictyService.GetElectricityDataAsync(date);

            return Json(electricityData); // Return the data as JSON
        }




        [HttpGet("ElDataGraph")]
        public async Task<IActionResult> ElDataGraph()
        {


            return View();

        }



       

        


        [HttpGet("WeatherDataGraph")]
        public async Task<IActionResult> WeatherDataGraph()
        {
            string tableName = "WeatherForecasts";
           
            
            var cities = new List<string> { "Linköping","Norrköping", "Finspång", "Stockholm" };
            DateTime startDate = new DateTime(2025, 1, 13);
            //DateTime endDate = new DateTime(2025, 1, 19);
            DateTime endDate = DateTime.UtcNow;
            
            // Fetch data for all cities
            var weatherData = await _weatherService.GetWeatherDataForCitiesAsync(tableName, cities, startDate, endDate);

            
            return View(weatherData);
        }


       

        

        [HttpGet("HourlyElectricity")]
        public async Task<IActionResult> HourlyElectricity(string date)
        {
            
            var electricityData = await _electrictyService.GetHourlyElAsync(date);

            return Json(electricityData);
        }






    }
}
