using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NewsProject.Models.DB
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name Field is required")]
        [StringLength(150, ErrorMessage = "Max Length is 150")]
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }

        [NotMapped]
        public IFormFile File { get; set; }
        public List<OrderRow> OrderRowList { get; set; } = new List<OrderRow>();
    }
}
