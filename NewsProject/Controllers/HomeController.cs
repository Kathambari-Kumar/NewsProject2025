using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using NewsProject.Data;
using NewsProject.Models;
using NewsProject.Models.DB;
using NewsProject.Services;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Azure.AI.OpenAI.Chat;


namespace NewsProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISubscriptionService _subscriptionService;

        private static List<ChatMessage> chatHistory = new List<ChatMessage>();

        public HomeController(ILogger<HomeController> logger, 
            UserManager<User> userManager,
            ApplicationDbContext dbContext,
            SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor,
            ISubscriptionService subscriptionService)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _subscriptionService = subscriptionService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CheckAccessLevel(int Id)
        {
            //var article = _adminService.GetArticleById(Id);
            var articleDetails = _dbContext.Articles
            .Where(a => a.Id == Id)
                           .GroupJoin(_dbContext.Categories,
                                    a => a.Category.Id,
                                    c => c.Id,
                                    (a, c) => new
                                    {
                                        Name = a.Category.Name,
                                    })
                            .ToList();
            string categoryName = articleDetails[0].Name;
            string message;
            if (categoryName == "Technology" || categoryName == "Weather")
            {
                Response.Cookies.Delete("TechnologyVisits");
                var visitStringTechnology = Request.Cookies["TechnologyVisits"];
                int TechnologyVisits = 0;
                int.TryParse(visitStringTechnology, out TechnologyVisits);
                TechnologyVisits++;
                CookieOptions cookie = new CookieOptions();
                cookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Append("TechnologyVisits", TechnologyVisits.ToString(), cookie);

                ViewBag.Visits = TechnologyVisits;
                if (TechnologyVisits <= 10 || _signInManager.IsSignedIn(User))
                {
                    //return RedirectToAction("DetailNewsDisplay", "NewsArticle", new { id = Id });
                    message = "signedIn";
                    return Json(message);
                }
                else
                {
                    message = "notSignedIn";
                    return Json(message);
                }
            }
            else if (categoryName == "Sports" || categoryName == "Economy" || categoryName == "World")
            {
                var visitStringSample = Request.Cookies["SampleVisits"];
                int sampleVisits = 0;
                int.TryParse(visitStringSample, out sampleVisits);
                sampleVisits++;
                CookieOptions cookie = new CookieOptions();
                cookie.Expires = DateTime.Now.AddDays(30);
                Response.Cookies.Append("SampleVisits", sampleVisits.ToString(), cookie);
                var session = _httpContextAccessor.HttpContext?.Session;
                var hasActiveSubscription = session != null && session.GetString("HasActiveSubscription") == "True";
                ViewBag.Visits = sampleVisits;
                if (sampleVisits <= 50 || _signInManager.IsSignedIn(User))
                {
                    message = "signedIn";
                    return Json(message);
                }
                else if (_signInManager.IsSignedIn(User) && hasActiveSubscription)
                {
                    // return RedirectToAction("DetailNewsDisplay", "NewsArticle", new { id = Id });
                    var user = _userManager.GetUserAsync(User).Result;
                    string subscriptionName = _subscriptionService.GetSubscriptionName(user);
                    if (subscriptionName == "Basic" && categoryName == "Sports")
                    {
                        message = "hasActiveSubscription";
                        return Json(message);
                    }
                    else if (subscriptionName == "Premium" && (categoryName == "Sports" || categoryName == "Economy"))
                    {
                        message = "hasActiveSubscription";
                        return Json(message);
                    }
                    else if (subscriptionName == "Gold")
                    {
                        message = "hasActiveSubscription";
                        return Json(message);
                    }
                    else
                    {
                        message = "noActiveSubscription";
                        return Json(message);
                    }
                }
                else
                {
                    message = "noActiveSubscription";
                    return Json(message);
                }
            }
            message = "signedIn";
            return Json(message);
        }

        public IActionResult SpeechToText()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetSpeechInput()
        {
            var speechKey = "DLlfXEDJh9HqyS1Xl9oGJfHUwUCd5cuS43lRp4rMDfeXdlssu5P5JQQJ99BAACfhMk5XJ3w3AAAYACOGcWKZ";
            var speechRegion = "swedencentral";
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.EndpointId = "https://swedencentral.api.cognitive.microsoft.com/";
            // default en-us
            speechConfig.SpeechRecognitionLanguage = "en-US";

            // configuring audio 
            // input from microphone - FromDefaultMicrophoneInput
            // input from file - FromWavFileInput 
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            
            //using var audioConfig = AudioConfig.FromWavFileInput("harward.wav");
            
            // create a SpeechRecognizer instance to speech from audio source
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);
            var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
            Console.WriteLine(speechRecognitionResult.Text);
            return Json(speechRecognitionResult.Text);
        }

        [HttpGet]
        public IActionResult ChatBox()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChatBox(string request)
        {
            // Configure OpenAI Client
            var endpoint = new Uri("https://gr2408openai.openai.azure.com/");
            var credential = new AzureKeyCredential("DMmUOCoi5RzOiVId58wNrJUrKbpAB9s7p022YfXRUzdhC9DS63xUJQQJ99BAACYeBjFXJ3w3AAABACOGiCD5");
            var model = "gpt-35-turbo";
            // create a new instance to connect the specific service resource endpoint
            var client = new AzureOpenAIClient(endpoint, credential);
            var chatClient = client.GetChatClient(model); // new chatclient instance

            var messages = new List<ChatMessage>();
            messages.Add(new SystemChatMessage("You are an AI assistant that helps people find information."));
            messages.Add(new UserChatMessage(request));
            var response = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions()
            {
                Temperature = (float)0.7, // Control the randomness of the output
                FrequencyPenalty = (float)0, // to diversify language use
                PresencePenalty = (float)0, // prevent the model from repeating the word
            });
            var chatResponse = response.Value.Content.Last().Text;
            return Json(chatResponse);
        }

        public IActionResult InteractiveChat()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InteractiveChat(string request)
        {
            // Configure OpenAI Client
            var endpoint = new Uri("https://gr2408openai.openai.azure.com/");
            var credential = new AzureKeyCredential("DMmUOCoi5RzOiVId58wNrJUrKbpAB9s7p022YfXRUzdhC9DS63xUJQQJ99BAACYeBjFXJ3w3AAABACOGiCD5");
            var model = "gpt-35-turbo";
            // create a new instance to connect the specific service resource endpoint
            var client = new AzureOpenAIClient(endpoint, credential);
            var chatClient = client.GetChatClient(model); // new chatclient instance

            // add user request to chathistory
            chatHistory.Add(new UserChatMessage(request));

            //get AI Completion
            var response = await chatClient.CompleteChatAsync(chatHistory);

            // add AI Completion to History
            chatHistory.Add(response.Value.Content[0].Text);

            return Json(response.Value.Content[0].Text);
        }

        public IActionResult ChatWithOwnData()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChatWithOwnData(string request)
        {
            // Configure OpenAI Client
            var endpoint = new Uri("https://gr2408openai.openai.azure.com/");
            var credential = new AzureKeyCredential("DMmUOCoi5RzOiVId58wNrJUrKbpAB9s7p022YfXRUzdhC9DS63xUJQQJ99BAACYeBjFXJ3w3AAABACOGiCD5");
            var model = "gpt-35-turbo";
            // create a new instance to connect the specific service resource endpoint
            var client = new AzureOpenAIClient(endpoint, credential);
            var chatClient = client.GetChatClient(model); // new chatclient instance

            // configure Search services 

            var searchEndpoint = "https://azure-oai-gr24-08.search.windows.net";
            string searchKey = "mDRwPWtS93Jz09xAxk5w46pHdqHVtKUul8Ihkos3moAzSeAbZsK1";
            string searchIndex = "dd-document-index";

            ChatCompletionOptions options = new();
            #pragma warning disable AOAI001
            options.AddDataSource(new AzureSearchChatDataSource()
            {
                Endpoint = new Uri(searchEndpoint),
                IndexName = searchIndex,
                Authentication = DataSourceAuthentication.FromApiKey(searchKey),
            });

            ChatCompletion completion = chatClient.CompleteChat([new UserChatMessage(request)], options);
            // class : ChatMessageContext
            // properties 
            // citations : The citations produced by the data retrieval.
            // intent : The detected intent from the chat history,
            // which is used to carry conversation context between interactions.
            // Retrieved Documents : Summary information about documents retrieved
            // by the data retrieval operation.

            ChatMessageContext messageContext = completion.GetMessageContext();
            string response = "";
            foreach (ChatCitation citation in messageContext.Citations)
            {
                response = response + citation.Content;
            }
            
            return Json(response);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Podcast()
        {
            return View();
        }

       public IActionResult DesignLayout()
       {
            return View();
       }

       public IActionResult UserStory(int page=1)
       {
            var images = new List<string>
       {
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory1.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory2.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory3.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory4.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory5.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory6.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory7.jpg",
        "https://dragonsstorage24.blob.core.windows.net/dragoncontainer/UserStory8.jpg"
       };

            int itemsPerPage = 4;

            var pagedImages = images.Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            int totalPages = (int)Math.Ceiling(images.Count / (double)itemsPerPage);

            ViewBag.Images = pagedImages;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View();

        }

        public IActionResult ShowCookies()
        { 
            
            return View(); 
        
        }



        //author: david345@hotmail.com
        //admin: annanilsson@gmail.com
        //reader: mm@mm.se
        //password for author,admin and reader: Mango1#

        //editor:achiengpauline2@gmail.com
        //password:Pauline2024@





       

    }
}
