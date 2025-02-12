namespace NewsProject.Models.DB
{
    public class ImageTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;


        public Article? Article { get; set; }


        
    }
}
