using System.ComponentModel.DataAnnotations;

namespace NewsProject.Models.DB
{
    public class Subscription
    {
        public int Id { get; set; }
        public SubscriptionType? SubscriptionType { get; set; }

        [StringLength(150)]
        public required double Price { get; set; }

        [StringLength(100)]
        public required DateTime Created { get; set; }

        [StringLength(100)]
        public DateTime Expiry { get; set; }
        public User? User { get; set; }
        public bool PaymentComplete { get; set; }
    }
}
