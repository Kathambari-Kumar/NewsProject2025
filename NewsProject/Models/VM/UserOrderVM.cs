namespace NewsProject.Models.VM
{
    public class UserOrderVM
    {
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public int OrderID { get; set; }
        public string CustomerName => $"{Firstname} {Lastname}";
        public string DateOfPurchase { get; set; } = string.Empty;
        public List<ProductVM>? Products { get; set; }
        public double TotalOrderCost { get; set; }
        public int TotalOrderCount { get; set; }
    }
}
