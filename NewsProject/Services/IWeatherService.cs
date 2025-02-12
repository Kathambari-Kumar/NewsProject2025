using NewsProject.Models.API;

namespace NewsProject.Services
{
    public interface IWeatherService
    {
        Task<List<WeatherData>> GetWeatherForecastAsync(string city, string lang);
        Task SaveWeatherDataAsync(WeatherData weatherData);

        Task<List<WeatherData>> GetWeatherForecastsForCitiesAsync(List<string> cities, string lang);

        Task<List<WeatherData>> GetWeatherDataForCitiesAsync(string tableName, List<string> cities, DateTime startDate, DateTime endDate);

    }
}
