using NewsProject.Models.API;

namespace NewsProject.Models.VM
{
    public class ElectricityVM
    {

        public List<string> ForecastDates { get; set; }  
        public List<decimal> PriceZone1 { get; set; }    
        public List<decimal> PriceZone2 { get; set; }    
        public List<decimal> PriceZone3 { get; set; }    
        public List<decimal> PriceZone4 { get; set; }
        public List<ElectricityData> ElectricityData { get; set; }
    }
}
