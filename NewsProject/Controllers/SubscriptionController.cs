using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsProject.Models.DB;
using NewsProject.Models.VM;
using NewsProject.Services;
using Newtonsoft.Json;
using Stripe.Checkout;

namespace NewsProject.Controllers
{
    public class SubscriptionController : Controller
    {
        public readonly ISubscriptionService _subscriptionService;
        public readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubscriptionController> _logger;
        private readonly IConfiguration _configuration;
        public SubscriptionController(ISubscriptionService subscriptionService,
            UserManager<User> userManager, ILogger<SubscriptionController> logger,
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
           
            _configuration = configuration;
        }

        /// <summary>
        /// ADMIN Functionalities
        /// </summary>
        /// <returns></returns>
        public IActionResult AddSubscriptionType()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddSubscriptionType(SubscriptionType subscriptionType)
        {

            if (subscriptionType == null || string.IsNullOrEmpty(subscriptionType.TypeName))
            {
                ModelState.AddModelError("", "Subscription type details are invalid.");
                return View(subscriptionType); // Return to the same form with error messages.
            }
            _subscriptionService.AddSubscriptionType(subscriptionType);
             //return RedirectToAction("AddSubscriptionType");
            TempData["Result"] = "The Subscription Type is added successfully!!";
            return RedirectToAction("AdminEditorAuthorFrontPage", "Admin");
        }

        [HttpGet]
        public IActionResult EditSubscriptionType(int id)
        {
            var subscriptionType = _subscriptionService.GetSubscriptionByID(id);
            return View(subscriptionType);
        }

        [HttpPost]
        public IActionResult EditSubscriptionType(SubscriptionType subscriptionType)
        {
            _subscriptionService.SaveSubscriptionType(subscriptionType);
            TempData["Result"] = "The Subscription Type is updated successfully!!";
            return RedirectToAction("AddSubscriptionType", "Admin");
        }

        [HttpGet]
        public IActionResult DeleteSubscriptionType(int id)
        {
            var subscriptionType = _subscriptionService.GetSubscriptionByID(id);
            return View(subscriptionType);
        }

        [HttpPost]
        public IActionResult DeleteSubscriptionType(SubscriptionType subscriptionType)
        {
            var result = _subscriptionService.DeleteSubscriptionType(subscriptionType);
            if (result == "Success")
                TempData["Result"] = "The Subscription Type is deleted successfully!!";
            else
                TempData["Result"] = "Delete Failed!!";

            return RedirectToAction("AddSubscriptionType");
        }

        // admin can view the News site's subscription details
        // chart form applied
        public IActionResult SubscriptionChart()
        {
            var userSubscriptionList = _subscriptionService.ViewAllSubscrptions();
            var chartDataList = _subscriptionService.MonthBasedUserCount();
            List<string> monthDataList = new List<string>();
            List<int> userCountList = new List<int>();
            foreach (var item in chartDataList)
            {
                monthDataList.Add(item.Month);
                userCountList.Add(item.UserCount);
            }
            TempData["MonthNameList"] = string.Join(",", monthDataList);
            TempData["UserCountList"] = string.Join(",", userCountList);
            return View(userSubscriptionList);
        }

        /// <summary>
        /// USER Functionalities
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Subscription()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var SubscriptionTypes = _subscriptionService.GetAllSubscriptionTypes();
            return View(SubscriptionTypes);
        }
        public IActionResult SubscriptionCheckout(int subscriptionTypeId)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var subscriptionDetails = _subscriptionService.GetSubscriptionByID(subscriptionTypeId);

            var domain = "https://dragonnews.azurewebsites.net/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Subscription/SubscribeNow?subscriptionTypeId={subscriptionTypeId}",
                CancelUrl = domain + "Home/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail = user.Email,
            };

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(subscriptionDetails.Price * 100),
                    Currency = "sek",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = subscriptionDetails.TypeName.ToString(),
                    }
                },
                Quantity = 1,
            };
                
            options.LineItems.Add(sessionLineItem);

            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public async Task<IActionResult> SubscribeNow(int subscriptionTypeId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = _subscriptionService.CreateSubscription(user, subscriptionTypeId);

            //it goes to the Azure for sending email
            var name = user.FirstName;
            var email = user.Email;
            var sendMsg = await _subscriptionService.SendEmailSubscriptionAsync(name, email);
            if (result)
            {
                //it shows on the screen
                TempData["Message"] = "success";
                bool hasActiveSubscription = true;
                _httpContextAccessor.HttpContext.Session.SetString("HasActiveSubscription", hasActiveSubscription.ToString());
            }
            else
            {
                TempData["Message"] = "danger";
            }

            return View();
        }

        // the user can view their subscription history
        public IActionResult ViewSubscriptionsList()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var subscription = _subscriptionService.ViewSubscriptionList(user.Id);
            if (subscription == null)
                TempData["NoSubscriptionHistory"] = "NoSubscriptionHistory";
            return View(subscription);
        }

        // upgrade the current subscription
        public IActionResult UpgradeSubscription()
        {
            var user =  _userManager.GetUserAsync(User).Result;
   
            // Fetch the user's current active subscription
            var activeSubscription = _subscriptionService.GetActiveSubscription(user.Id);

            var currentSubscriptionVM = new SubscriptionVM
            {
                Id = activeSubscription.Id,
                SubscriptionType = activeSubscription.SubscriptionType.TypeName,
                Description = activeSubscription.SubscriptionType.Description,
                Price = activeSubscription.SubscriptionType.Price,
                IsActive = true
            };

            ViewBag.ActiveSubscription = currentSubscriptionVM;
            return View(currentSubscriptionVM);
        }


        [HttpPost]
        public IActionResult UpgradeSubscription(int months)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var activeSubscription = _subscriptionService.GetActiveSubscription(user.Id);
            var subscriptionName = _subscriptionService.GetSubscriptionName(user);

            double basePrice = activeSubscription.SubscriptionType.Price;
            double totalPrice = months * basePrice;
            if (months >= 6 && months < 12)
            {
                totalPrice *= 0.95; // 5% discount for 6+ months
            }
            else if (months >= 12)
            {
                totalPrice *= 0.90; // 10% discount for 12+ months
            }

            var domain = "https://dragonnews.azurewebsites.net/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Subscription/UpgradeNow?months={months}",
                CancelUrl = domain + "Home/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail = user.Email,
            };

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(totalPrice * 100),
                    Currency = "sek",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = subscriptionName.ToString(),
                    }
                },
                Quantity = 1,
            };

            options.LineItems.Add(sessionLineItem);

            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult UpgradeNow(int months)
        {
            var user = _userManager.GetUserAsync(User).Result;
            var activeSubscription = _subscriptionService.GetActiveSubscription(user.Id);
            var newExpiryDate = activeSubscription.Expiry.AddMonths(months);
            // Base price per month
            double basePrice = activeSubscription.SubscriptionType.Price;

            // Calculate the total price
            double totalPrice = months * basePrice;

            // Apply discounts
            if (months >= 6 && months < 12)
            {
                totalPrice *= 0.95; // 5% discount for 6+ months
            }
            else if (months >= 12)
            {
                totalPrice *= 0.90; // 10% discount for 12+ months
            }
            var updatedSubscription = _subscriptionService.UpgradeSubscription(user, newExpiryDate, totalPrice);
            if (updatedSubscription == "error")
            {
                TempData["Message"] ="danger";
                return View();
            }

            var name = user.FirstName;
            var email = user.Email;

            var result =  _subscriptionService.SendUpgradeMessageAsync(name,email).Result;
            TempData["Message"] = "success";
            return View();
        }

        public IActionResult SuccessUpgrade()
        {
            return View();
        }
    }
}
