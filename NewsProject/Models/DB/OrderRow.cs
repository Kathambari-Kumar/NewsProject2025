namespace NewsProject.Models.DB
{
    public class OrderRow
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public Product? Product { get; set; }
        public Order? Order { get; set; }
    }
}
