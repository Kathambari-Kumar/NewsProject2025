using Azure;
using Azure.Data.Tables;

namespace NewsProject.Models.API
{
    public class ElectricityData 
    {
            public string Date { get; set; }
            public Dictionary<string, Zone[]> Zones { get; set; }
        

        public class Zone
        {
            public int Hour { get; set; }
            public float Price_eur { get; set; }
            public float Price_sek { get; set; }
            public int KMeans { get; set; }
        }




    }
}
