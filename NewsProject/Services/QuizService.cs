using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.DB;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using System.Text.RegularExpressions;

namespace NewsProject.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        public QuizService(ApplicationDbContext context)
        {
            _context = context;
            
        }

       
        // Fetch a question by its ID (for navigation)
        public async Task<List<Quiz>> GetQuizzesByIdsAsync(List<int> ids)
        {
            return await _context.Quizzes.Where(q => ids.Contains(q.Id)).ToListAsync();
        }

        public async Task<List<int>> GetAllQuizIdsAsync()
        {
            return await _context.Quizzes.Select(q => q.Id).ToListAsync();
        }

        public async Task<Quiz> GetQuizByIdAsync(int id)
        {
            // Find the quiz with the given ID from the database
            var quiz = await _context.Quizzes
                .Where(q => q.Id == id) // Filter by ID
                .FirstOrDefaultAsync(); // Get the first match or null if not found

            return quiz; // Return the quiz object, or null if not found
        }
        public async Task CreateQuizAsync(Quiz quiz)
        {
              _context.Quizzes.Add(quiz);
              await  _context.SaveChangesAsync();
             
        }    
        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();             
        }
        public async Task DeleteQuizAsync(Quiz quiz)
        {
            _context.Remove(quiz);
            await _context.SaveChangesAsync();          
        }
        public async Task<List<Quiz>> GetAllQuizListAsync()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return quizzes;
        }
    }
}
