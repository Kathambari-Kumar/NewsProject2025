namespace NewsProject.Models.API
{
    public class TomorrowWeatherForecast
    {
        public Data data { get; set; }
        public Location location { get; set; }
    }

    public class Data
    {
        public DateTime time { get; set; }
        public Values values { get; set; }
    }

    public class Values
    {
        public float cloudBase { get; set; }
        public float cloudCeiling { get; set; }
        public int cloudCover { get; set; }
        public float dewPoint { get; set; }
        public int freezingRainIntensity { get; set; }
        public float hailProbability { get; set; }
        public float hailSize { get; set; }
        public int humidity { get; set; }
        public int precipitationProbability { get; set; }
        public float pressureSurfaceLevel { get; set; }
        public float rainIntensity { get; set; }
        public int sleetIntensity { get; set; }
        public int snowIntensity { get; set; }
        public float temperature { get; set; }
        public float temperatureApparent { get; set; }
        public int uvHealthConcern { get; set; }
        public int uvIndex { get; set; }
        public float visibility { get; set; }
        public int weatherCode { get; set; }
        public float windDirection { get; set; }
        public float windGust { get; set; }
        public float windSpeed { get; set; }
    }

    public class Location
    {
        public float lat { get; set; }
        public float lon { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }
}
