using NewsProject.Models.API;

namespace NewsProject.Models.VM
{
    public class WeatherVM
    {
        public List<WeatherData> WeatherForecasts { get; set; } = new List<WeatherData>();
        public List<string> Cities { get; set; } = new List<string> { "Linköping", "Norrköping", "Finspång", "Stockholm" };


    }
}
