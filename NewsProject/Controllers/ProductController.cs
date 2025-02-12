using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsProject.Models.DB;
using NewsProject.Extensions;
using NewsProject.Models.VM;
using NewsProject.Services;
using Newtonsoft.Json;
using Stripe.Checkout;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace NewsProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ProductController(IProductService productService,
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _productService = productService;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            // Blob Storage Process
            string imgurl = string.Empty;
            string connectionString = _configuration["AzureBlobConnectionString"];
            string containerName = _configuration["AzureBlobContainerName"];
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();

            BlobClient blobClient = containerClient.GetBlobClient(product.File.FileName);
            var extension = Path.GetExtension(product.File.FileName);
            Stream outputStream = new MemoryStream();
            using (var stream = product.File.OpenReadStream())
            {
                using (Image image = Image.Load(stream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Stretch,
                        Size = new Size(800, 350)
                    }));
                    var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                    image.Save(outputStream, jpegEncoder);
                    outputStream.Position = 0;
                    blobClient.Upload(outputStream, true);
                    stream.Dispose();
                    imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                }
            }
            product.ImageUrl = imgurl; // store the URL as ImageLink
            var returnMessage = _productService.CreateProduct(product);
            TempData["Result"] = "The Product is added successfully!!";
            return RedirectToAction("AdminEditorAuthorFrontPage", "Admin");
        }

        public IActionResult ProductDetails()
        {
            var productList = _productService.GetProductDetails();
            return View(productList);
        }

        [HttpGet]
        public IActionResult UpdateProduct(int Id)
        {
            var product = _productService.GetProductById(Id);
            return View(product);
        }

        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {
            if (product.File != null)
            {
                string imgurl = string.Empty;
                string connectionString = _configuration["AzureBlobConnectionString"];
                string containerName = _configuration["AzureBlobContainerName"];
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                containerClient.CreateIfNotExists();

                BlobClient blobClient = containerClient.GetBlobClient(product.File.FileName);

                Stream outputStream = new MemoryStream();
                using (var stream = product.File.OpenReadStream())
                {
                    using (Image image = Image.Load(stream))
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Stretch,
                            Size = new Size(800, 350)
                        }));
                        var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder();
                        image.Save(outputStream, jpegEncoder);
                        outputStream.Position = 0;
                        blobClient.Upload(outputStream, true);
                        stream.Dispose();
                        imgurl = blobClient.Uri.AbsoluteUri; // get the URL
                    }
                }
                product.ImageUrl = imgurl; // store the URL as ImageLink            
            }
            var message = _productService.UpdateProduct(product);
            TempData["Result"] = "The Product is updated successfully!!";
            return RedirectToAction("ProductDetails"); ;
        }

        [HttpGet]
        public IActionResult DeleteProduct(int Id)
        {
            var message = _productService.DeleteProduct(Id);
            TempData["Result"] = "The Product is deleted successfully!!";
            return RedirectToAction("ProductDetails");
        }

        public IActionResult DisplayProducts()
        {
            var productList = _productService.GetProductDetails();
            return View(productList);
        }

        // Cart Functionalities

        [HttpGet]
        public IActionResult AddToCart(int productId)
        {
            return View();
        }
        [HttpPost, ActionName("AddToCart")]
        public IActionResult AddToShoppingCart(int productId, [Bind("Id, ProductName,Price, ImageUrl")] Product product)
        {
            var cartList = HttpContext.Session.Get<List<int>>("ShoppingCart") ?? new List<int> { };
            cartList.Add(productId); //add the movieId to the list
            _productService.AddProductToCart(product.Id);
            var numberOfListItems = cartList.Count(); //counts and adds the count to numberoflistitems
            HttpContext.Session.Set<List<int>>("ShoppingCart", cartList); //reset the shopping list and store in session                
            return Json(numberOfListItems);
        }

        public IActionResult CheckUserStatus()
        {
            if (_signInManager.IsSignedIn(User))
                return Json("SignedIn");
            else
                return Json("Failed");
        }
        public IActionResult DisplayCart()
        {
            var productCartItems = _productService.DisplayCart();
            if (productCartItems.Count > 0)
            {
                return View(productCartItems);
            }
            else
            {
                return View();
            }
        }

        public IActionResult Checkout()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var httpsession = _httpContextAccessor.HttpContext.Session;
            List<CartItem>? cartitems = JsonConvert.DeserializeObject<List<CartItem>>(httpsession.GetString("ProductCart"));
            var domain = "https://dragonnews.azurewebsites.net/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + "Product/OrderConfirmation",
                CancelUrl = domain + "Product/DisplayCart",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail = user.Email,
            };

            foreach (var item in cartitems)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * item.Copies) * 100,
                        Currency = "sek",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name.ToString(),
                        }
                    },
                    Quantity = item.Copies,
                };
                options.LineItems.Add(sessionLineItem);

            }
            var service = new SessionService();
            Session session = service.Create(options);
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }
        public IActionResult OrderConfirmation()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var message = _productService.UserCheckout(user.Email);
            TempData["ReturnMessage"] = message.Result.ToString();
            return View();
        }

        public IActionResult GettingCartCount()
        {
            var cartList = HttpContext.Session.Get<List<int>>("ShoppingCart");
            var numberOfListItems = cartList.Count();
            HttpContext.Session.Set<List<int>>("ShoppingCart", cartList);
            return Json(numberOfListItems);
        }
        public IActionResult AddItem(int productId)
        {
            _productService.IncreaseCopy(productId);
            return View();
        }

        public IActionResult ReduceItem(int productId)
        {
            _productService.DecreaseCopy(productId);
            return View();
        }
        public IActionResult GetUsersOrderDetails()
        {
            var user = _userManager.GetUserAsync(User).Result;
            var orderDetailList = _productService.GetUsersOrders(user.Email);
            if (orderDetailList.Count <= 0)
                TempData["NoOrderYet"] = "No Order";
            return View(orderDetailList);
        }
    }
}
