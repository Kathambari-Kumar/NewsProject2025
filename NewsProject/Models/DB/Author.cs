namespace NewsProject.Models.DB
{
    public class Author
    {
        public int Id { get; set; }
        public string UserID { get; set; } = string.Empty;
        public Article? Article { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
