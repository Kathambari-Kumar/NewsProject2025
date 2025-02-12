using System.ComponentModel.DataAnnotations;

namespace NewsProject.Models.DB 
{
    public class Category
    {
        public int Id { get; set; }

        [StringLength(150)]
        public string Name { get; set; } = string.Empty;
        public List<Article>? Articles { get; set; }
    }
}
