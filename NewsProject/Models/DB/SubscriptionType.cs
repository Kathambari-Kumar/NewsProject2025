using System.ComponentModel.DataAnnotations;

namespace NewsProject.Models.DB
{
    public class SubscriptionType
    {
        public int Id { get; set; }

        [StringLength(150)]
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public List<Subscription>? Subscriptions { get; set; }
    }
}
