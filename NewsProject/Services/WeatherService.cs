using Azure.Data.Tables;
using Microsoft.VisualBasic;
using NewsProject.Models.API;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewsProject.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;
        private readonly HttpClient _httpClient;

        public WeatherService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["AzureBlobConnectionString"];
            _tableServiceClient = new TableServiceClient(_connectionString);
            _tableClient = _tableServiceClient.GetTableClient(tableName: "DragonWeatherForecasts");
            _httpClient = new HttpClient();
        }

        public async Task<List<WeatherData>> GetWeatherForecastAsync(string city, string lang)
        {
            var url = $"http://weatherapi.dreammaker-it.se/forecast?city={city}&lang={lang}";
            var response = await _httpClient.GetStringAsync(url);           

            // Log the response for debugging
            Console.WriteLine(response);

            var weatherForecast = new List<WeatherData>();
            try
            {
                var forecast = JsonConvert.DeserializeObject<WeatherData>(response); // For single object
                forecast.Timestamp = DateTime.UtcNow; // Set current UTC time
                weatherForecast.Add(forecast);
            }
            catch (JsonSerializationException)
            {
                weatherForecast = JsonConvert.DeserializeObject<List<WeatherData>>(response); // For array
                foreach (var forecast in weatherForecast)
                {
                    forecast.Timestamp = DateTime.UtcNow; // Set current UTC time
                }
            }

            return weatherForecast;
        }

        public async Task SaveWeatherDataAsync(WeatherData weatherData)
        {
            if (weatherData.Timestamp == null)
            {
                weatherData.Timestamp = DateTime.UtcNow; // Set to current time if null
            }

            weatherData.PartitionKey = weatherData.city;
            weatherData.RowKey = $"{weatherData.city}_{weatherData.Timestamp:yyyyMMddHHmmss}";
            //weatherData.Timestamp?.ToString("yyyyMMddHHmmss")
            //         ?? Guid.NewGuid().ToString();
           
            _tableClient.CreateIfNotExists();
          
            await _tableClient.AddEntityAsync(weatherData);
        }


        public async Task<List<WeatherData>> GetWeatherForecastsForCitiesAsync(List<string> cities, string lang)
        {
            var allWeatherForecasts = new List<WeatherData>();

            foreach (var city in cities)
            {
                // Fetch weather data for each city
                var forecasts = await GetWeatherForecastAsync(city, lang);
                allWeatherForecasts.AddRange(forecasts);
            }

            return allWeatherForecasts;
        }

        public async Task<List<WeatherData>> GetWeatherDataForCitiesAsync(string tableName, List<string> cities, DateTime startDate, DateTime endDate)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var results = new List<WeatherData>();

            foreach (var city in cities)
            {
                var query = tableClient.QueryAsync<WeatherData>(
                    filter: $"PartitionKey eq '{city}' and Timestamp ge datetime'{startDate:O}' and Timestamp le datetime'{endDate:O}'",
                    maxPerPage: 100);

                await foreach (var item in query)
                {
                    results.Add(item);
                }
            }
            return results;
        }
    }
}

