using Azure.Data.Tables;
using NewsProject.Models.API;
using Newtonsoft.Json;
using static NewsProject.Models.API.ElectricityData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NewsProject.Services
{
    public class ElectricityService : IElectrictyService
    {

        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient1;
        private readonly TableClient _tableClient;
        private readonly string _connectionString;
        private readonly TableServiceClient _tableServiceClient;
        private readonly IConfiguration _configuration;

        public ElectricityService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _connectionString = _configuration["AzureBlobConnectionString"];
            _tableServiceClient = new TableServiceClient(_connectionString);
            _tableClient = _tableServiceClient.GetTableClient(tableName: "DragonElectricity");
            _httpClient = httpClientFactory.CreateClient("espot");
            _httpClient1 = httpClientFactory.CreateClient("espot");
        }
        
        
        public async Task<Electricity> GetElectricityDataAsync(string date)
        {
            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.UtcNow.ToString("yyyy-MM-dd"); // Default to the current date
            }

            string url = $"espot/{date}"; // Dynamic date
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error fetching data from API. Status code: {response.StatusCode}");
            }
            string json = await response.Content.ReadAsStringAsync();
            Electricity electricityData = JsonConvert.DeserializeObject<Electricity>(json);
            return electricityData;
          }            
              
       public async Task<ElectricityData> GetHourlyElAsync(string date)
       {

            try
            {
                var url = $"https://spotprices.lexlink.se/espot?format=json&date={date}";
                var response = await _httpClient1.GetAsync(url);


                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the JSON response
                var json = await response.Content.ReadAsStringAsync();

                // Deserialize JSON to raw model
                var rawData = JsonConvert.DeserializeObject<ElectricityRaw>(json);
                if (rawData == null)
                {
                    throw new Exception("Failed to deserialize API response.");
                }

                // Map raw data to the optimized ElectricityData model
                var zones = new Dictionary<string, Zone[]>
                {
                    { "SE1", rawData.SE1 },
                    { "SE2", rawData.SE2 },
                    { "SE3", rawData.SE3 },
                    { "SE4", rawData.SE4 }
                };

                return new ElectricityData
                {
                    Date = rawData.Date,
                    Zones = zones
                };
            }
            catch (HttpRequestException httpEx)
            {
                // Log or handle network errors
                throw new Exception($"Network error while fetching data: {httpEx.Message}", httpEx);
            }
            catch (Exception ex)
            {
                // Log or handle other errors
                throw new Exception($"Unexpected error: {ex.Message}", ex);
            }
       }

        public class ElectricityRaw
        {
            public string Date { get; set; }
            public Zone[] SE1 { get; set; }
            public Zone[] SE2 { get; set; }
            public Zone[] SE3 { get; set; }
            public Zone[] SE4 { get; set; }
        }
    }
}

