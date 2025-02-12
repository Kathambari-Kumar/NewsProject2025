namespace NewsProject.Models.VM
{
    public class SubscriptionsListVM
    {

        public string UserName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string SubscriptionType { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expiry { get; set; }

    }
}
