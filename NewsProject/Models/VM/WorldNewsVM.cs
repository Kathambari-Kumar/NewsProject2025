using NewsProject.Models.DB;

namespace NewsProject.Models.VM
{
    public class WorldNewsVM
    {
        public string Continent { get; set; } = string.Empty;
        public List<Article>? articles { get; set; } = new List<Article>();
    }
}
