using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NewsProject.Data;
using NewsProject.Models.API;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using Newtonsoft.Json;
using System.Composition;
using System.Text;

namespace NewsProject.Services
{
    public class UserSubscriptionService : ISubscriptionService
    {

        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClient1;

        public UserSubscriptionService(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient1 = httpClientFactory.CreateClient();
        }   
        public async Task<bool> SendUpgradeMessageAsync(string name, string email)
        {
            try
            {
                //URL for your published httptriggered function
                var functionUrl = "https://upgradesubscription.azurewebsites.net/api/SendUpgradeMessage?code=oRMPIy4KD8ovJY53xgClPCioPkL0FpMwkOSWEeEY6ofdAzFu4NdxaQ%3D%3D";

                // Create an object for the request body
                var requestBody = new
                {
                    name = name,
                    email = email
                };

                // Convert the object to JSON
                var jsonBody = JsonConvert.SerializeObject(requestBody);

                // Prepare the request content with the JSON body
                var content = new StringContent(jsonBody, Encoding.UTF8);

                // Send the POST request
                var response = await _httpClient.PostAsync(functionUrl, content);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Log or handle the error response if needed
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the request
                Console.WriteLine($"Error calling the function: {ex.Message}");
                return false;
            }
        }

        // get all the subscription type details
        public List<SubscriptionType> GetAllSubscriptionTypes()
        {
            return _context.SubscriptionsTypes.ToList();
        }

        // Create a new subscription for user
        public bool CreateSubscription(User user, int subscriptionTypeId)
        {
      
            //check if subscription type exist
            // get the details of subscription type
            var subscriptiontype = _context.SubscriptionsTypes
                .FirstOrDefault(s => s.Id == subscriptionTypeId);

            // creating Subscription Class Object
            // and assign value to it's Properties

            var subscription = new Subscription()
            {
                SubscriptionType = subscriptiontype,
                Price = subscriptiontype.Price,
                Created = DateTime.Now,
                Expiry = DateTime.Now.AddMonths(1),
                PaymentComplete = true,
                User = user
            };

            // Add and save the subscription
            _context.Subscriptions.Add(subscription);
            _context.SaveChanges();

            // set IsNewsLetter field to True
            user.IsNewsLetter = true;
            _context.Update(user);
            _context.SaveChanges();
            return true;
        }

        // Adding New Subscription Plan
        public bool AddSubscriptionType(SubscriptionType subType)
        {
            _context.SubscriptionsTypes.Add(subType);
            _context.SaveChanges();
            return true;
        }

        // user can view their subscription history
        public List<SubscriptionsListVM> ViewSubscriptionList(string userId)
        {
            var activeSubscriptionList = _context.Subscriptions
                                .Where(s => s.User.Id == userId)
                                .Include(s => s.User) // Include user details
                               .Select(s => new SubscriptionsListVM()
                               {
                                   UserName = $"{s.User.FirstName} {s.User.LastName}",
                                   Email = s.User.Email,
                                   SubscriptionType = s.SubscriptionType.TypeName,
                                   Price = s.Price,
                                   Created = s.Created,
                                   Expiry = s.Expiry

                               }).ToList();
            return activeSubscriptionList;
        }
        // Month based users subscription list
        // for preparing chart
        public List<SubscriptionChartVM> MonthBasedUserCount()
        {
            var usercountlist = _context.Subscriptions
                                .GroupBy(s => s.Created.Month)
                                .Select(result => new
                                {
                                    month = result.Key,
                                    count = result.Select(r => r.User.Id)
                                                .Count()
                                }).ToList();

            List<SubscriptionChartVM> chartDataList = new List<SubscriptionChartVM>() {
               new SubscriptionChartVM{ Month= "Jan",UserCount = 0},
               new SubscriptionChartVM{ Month="Feb", UserCount = 0},
               new SubscriptionChartVM{ Month= "Mar",UserCount = 0},
               new SubscriptionChartVM{ Month="Apr", UserCount = 0},
               new SubscriptionChartVM{ Month= "May",UserCount = 0},
               new SubscriptionChartVM{ Month="Jun", UserCount = 0},
               new SubscriptionChartVM{ Month= "Jul",UserCount = 0},
               new SubscriptionChartVM{ Month="Aug", UserCount = 0},
               new SubscriptionChartVM{ Month= "Sep",UserCount = 0},
               new SubscriptionChartVM{ Month="Oct", UserCount = 0},
               new SubscriptionChartVM{ Month= "Nov",UserCount = 0},
               new SubscriptionChartVM{ Month="Dec", UserCount = 0}   };
            int j = 0;
            foreach (var item in usercountlist)
            {
                j = 0;
                for (int i = 1; i <= 12; i++)
                {
                    if (item.month == i)
                    {
                        chartDataList[j].UserCount = item.count;
                    }
                    j++;
                }
            }
            return chartDataList; // Month based user count
        }

        // admin can view subscription's statistics
        public List<SubscriptionsListVM> ViewAllSubscrptions()
        {
            var usersubscriptionlist = _context.Subscriptions
                                .Include(s => s.User) // Include user details
                               .Select(s => new SubscriptionsListVM()
                               {
                                   UserName = $"{s.User.FirstName} {s.User.LastName}",
                                   Email = s.User.Email,
                                   SubscriptionType = s.SubscriptionType.TypeName,
                                   Price = s.Price,
                                   Created = s.Created,
                                   Expiry = s.Expiry

                               }).ToList();
            return usersubscriptionlist;
        }

        // check if user has an active subscription
        // decide to display Subscribe button 
        public bool HasActiveSubscription(string userId)
        {
            return _context.Subscriptions
                    .Include(s => s.User)
                    .Any(s => s.User.Id == userId
                    && s.Expiry > DateTime.Now);
        }

        // get the active subscription for the user
        public Subscription GetActiveSubscription(string userId)
        { 
            var subscription = _context.Subscriptions
                          .Include(s => s.SubscriptionType)
                          .FirstOrDefault(s => s.User.Id == userId
                           && s.Expiry > DateTime.Now);
            return subscription;
        }
        public string UpgradeSubscription(User user, DateTime newExpiryDate, double totalPrice)
        {
            var subscription = _context.Subscriptions
                                   .Include(s => s.User)
                                   .FirstOrDefault(s => s.User == user 
                                   && s.Expiry > DateTime.Now);
            if (subscription != null)
            {
                subscription.Expiry = newExpiryDate;
                subscription.Price = totalPrice;
                _context.Subscriptions.Update(subscription);
                _context.SaveChanges();
                return "success";
            }
            else
                return "error";
        }

        public async Task<bool> SendEmailSubscriptionAsync(string name,  string email)
        {
            try
            {
                var functionurl = "https://sendsubscriptiontoqueue.azurewebsites.net/api/SendSubscriptionMessage?code=FoRxclUM4XtEKpcuRWv5uxPaFHTbmjaTpRKsOXJj63ZXAzFujiglIA%3D%3D";

                //create an object for the request body
                var requestBody = new
                {
                    Name = name,
                    Email = email,
                };

                //Convert the object to json
                var jsonBody = JsonConvert.SerializeObject(requestBody);

                //prepare the request content with the jsonBody
                var content = new StringContent(jsonBody, Encoding.UTF8);

                //Send the post request
                var response = await _httpClient1.PostAsync(functionurl, content);

                //check if the request was successfull
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    //log or handle the error response if needed
                    return false;
                }
            }
            catch (Exception ex)
            {
                //handle any exceptions that occur
                Console.WriteLine($"Error calling the functon : {ex: Message}");
                return false;
            }
        }
        public SubscriptionType GetSubscriptionByID(int id)
        {
            var subscription = _context.SubscriptionsTypes.FirstOrDefault(s=>s.Id == id);
            return subscription;
        }

        public void SaveSubscriptionType(SubscriptionType subscriptionType)
        {
            _context.Update(subscriptionType);
            _context.SaveChanges();

        }
        public string GetSubscriptionName(User user)
        {
            var subscription = _context.Subscriptions
                                   .Include(s => s.User)
                                   .FirstOrDefault(s => s.User == user);
            var subscriptionType = _context.SubscriptionsTypes
                                    .FirstOrDefault(s => s.Id == subscription.SubscriptionType.Id);
            return subscriptionType.TypeName;
        }
        public string DeleteSubscriptionType(SubscriptionType subscriptionType)
        {
            var userSubscription = _context.Subscriptions
                             .Where(s => s.SubscriptionType.Id == subscriptionType.Id
                                                && s.Expiry >= DateTime.Now)
                             .ToList();
            if (userSubscription == null)
            {
                _context.Remove(subscriptionType);
                _context.SaveChanges();
                return "Success";
            }
            else { return "Error"; }
        }
    }
}



