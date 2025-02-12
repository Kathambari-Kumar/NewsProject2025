using NewsProject.Models.DB;
using SixLabors.ImageSharp.Formats;

namespace NewsProject.Services
{
    public interface IQuizService
    {
        Task<List<int>> GetAllQuizIdsAsync();
        Task<List<Quiz>> GetQuizzesByIdsAsync(List<int> ids);
        Task<Quiz> GetQuizByIdAsync(int id);
        Task CreateQuizAsync(Quiz quiz);
        Task UpdateQuizAsync(Quiz quiz);
        Task DeleteQuizAsync(Quiz quiz);
        Task<List<Quiz>> GetAllQuizListAsync();
    }
}
