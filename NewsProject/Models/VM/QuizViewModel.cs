using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    public class QuizViewModel
    {
        public Quiz? Quiz { get; set; } // Assuming Quiz is your model for the quiz question
        public int CurrentQuestionNumber { get; set; }
    }
}
