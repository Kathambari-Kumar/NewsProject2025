using NewsProject.Models.API;
using NewsProject.Models.DB;
using NewsProject.Models.VM;

namespace NewsProject.Services
{
    public interface ISubscriptionService
    {
        public bool CreateSubscription(User user, int subscriptionTypeId);
        public bool AddSubscriptionType(SubscriptionType subType);
        public List<SubscriptionsListVM> ViewSubscriptionList(string userId);
        public List<SubscriptionChartVM> MonthBasedUserCount();
        public List<SubscriptionsListVM> ViewAllSubscrptions();
        public List<SubscriptionType> GetAllSubscriptionTypes();
        public string UpgradeSubscription(User user, DateTime newExpiryDate, double price);
        public Subscription GetActiveSubscription(string userId);
        public  bool HasActiveSubscription(string userId);
        Task<bool> SendUpgradeMessageAsync(string name, string email);
        Task<bool> SendEmailSubscriptionAsync(string name, string email);
        public SubscriptionType GetSubscriptionByID(int id);
        public void SaveSubscriptionType(SubscriptionType subscriptionType);
        public string DeleteSubscriptionType(SubscriptionType subscriptionType);
        public string GetSubscriptionName(User user);
        //public List<SubscriptionType> GetAllSubscription();
    }
}
