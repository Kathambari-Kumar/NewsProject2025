namespace NewsProject.Models.DB
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderDate { get; set; } = string.Empty;
        public User? User { get; set; }
        public List<OrderRow> OrderRowList { get; set; } = new List<OrderRow>();
    }
}
