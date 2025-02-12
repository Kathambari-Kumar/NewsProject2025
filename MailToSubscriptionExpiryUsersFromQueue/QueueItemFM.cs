using System;
using System.Collections.Generic;

namespace MailToSubscriptionExpiryUsersFromQueue
{
    public class QueueItemFM
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public List<SubscriptionType> SubscriptionTypes { get; set; } = new List<SubscriptionType>();
    }
}
