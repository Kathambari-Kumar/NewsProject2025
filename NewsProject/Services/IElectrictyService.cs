using NewsProject.Models.API;

namespace NewsProject.Services
{
    public interface IElectrictyService
    {

        Task<Electricity> GetElectricityDataAsync(string date);
       

        
        //Task<List<ElDataFlat>> GetElDataForZonesAsync(string tableName, List<string> zones, DateTime startDate, DateTime endDate);

        Task<ElectricityData> GetHourlyElAsync(string date);
    }
    
}
