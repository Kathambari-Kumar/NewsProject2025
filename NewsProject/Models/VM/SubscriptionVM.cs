namespace NewsProject.Models.VM
{
    public class SubscriptionVM
    {
        public int Id { get; set; }
        public string SubscriptionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty ;
        public double  Price { get; set; }
        public bool IsActive { get; set; }

    }
}
