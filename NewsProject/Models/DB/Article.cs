using System.ComponentModel.DataAnnotations.Schema;

namespace NewsProject.Models.DB
{
    public class Article
    {
        public int Id { get; set; }
        public DateTime DateStamp { get; set; }
        public string LinkText { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public string ContentSummary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
       
        public int Views { get; set; }
        public int Likes { get; set; }
        public string ImageLink { get; set; } = string.Empty;

        public string VoiceInput {  get; set; } = string.Empty;
        [NotMapped]
        public IFormFile File { get; set; }
        public Category? Category { get; set; }
        public string Continent { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public bool IsApproved { get; set; }
        public bool EditorChoice { get; set; }
        public List<Tag>? Tags{ get; set; }
        public User? User {  get; set; }
        public List<ImageTag> ImageTags { get; set; } = new List<ImageTag>();
    }
}
