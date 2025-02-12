using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace NewsProject.Models.API
{
   
    public class WeatherData : ITableEntity
    {
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; } = default!;
    public ETag ETag { get; set; } = default!;


    [JsonProperty("summary")]
    public string Summary { get; set; }
    public string city { get; set; }
    public object lang { get; set; }
    public int temperatureC { get; set; }
    public int temperatureF { get; set; }
    public int humidity { get; set; }
    public int windSpeed { get; set; }

    private DateTime _date;
    public DateTime date
    {
        get => _date;
        set => _date = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public int unixTime { get; set; }

    //public Icon icon { get; set; }

    // Flattened Icon Properties
    //public string IconUrl { get; set; }
    //public string IconCode { get; set; }


    }
}
