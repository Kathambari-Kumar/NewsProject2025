using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace NewsProject.Models.API
{
    public class WeatherForecast
    {
         [JsonProperty("summary")]
        public string Summary { get; set; }
        public string city { get; set; }
        public object lang { get; set; }
        public int temperatureC { get; set; }
        public int temperatureF { get; set; }
        public int humidity { get; set; }
        public int windSpeed { get; set; }
        
         public DateTime date { get; set; }
       
       
        public int unixTime { get; set; }
        
        public Icon icon { get; set; }
    }
    public class Icon
    {
        public string url { get; set; }
        public string code { get; set; }
    }
}
