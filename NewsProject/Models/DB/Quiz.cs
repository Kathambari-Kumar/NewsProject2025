using Microsoft.CodeAnalysis.Options;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsProject.Models.DB
{
    public class Quiz
    {
        public int Id { get; set; }
        public string HeadLine { get; set; } = string.Empty;
        public string ImageLink {  get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DateStamp { get; set; } 
        public string Question { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile File { get; set; }
    }
}
