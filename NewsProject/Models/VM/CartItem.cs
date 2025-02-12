namespace NewsProject.Models.VM
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Copies { get; set; }
    }
}
