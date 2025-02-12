using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit.Text;
using MimeKit;
using NewsProject.Data;
using NewsProject.Models.DB;
using NewsProject.Extensions;
using NewsProject.Models.VM;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats;
using System.Text.RegularExpressions;

namespace NewsProject.Services
{
    public class NotebookProductService : IProductService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        List<CartItem> CartItems = new List<CartItem>();

        public NotebookProductService(ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public string CreateProduct(Product product)
        {
            if (product != null)
            {
                product.IsAvailable = true;
                _dbContext.Products.Add(product);
                _dbContext.SaveChanges();
                return "Product Added";
            }
            else
            {
                return "Error";
            }
        }

        public List<Product> GetProductDetails()
        {
            return _dbContext.Products.ToList();
        }
        public Product GetProductById(int productId)
        {
            var product = _dbContext.Products
                            .FirstOrDefault(p => p.Id == productId);
            return product;
        }
        public string UpdateProduct(Product product)
        {
            if (product != null)
            {
                _dbContext.Products.Update(product);
                _dbContext.SaveChanges();
                return "Product Updated";
            }
            else
            {
                return "Error";
            }
        }

        public string DeleteProduct(int productId)
        {
            var product = _dbContext.Products
                            .FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                _dbContext.SaveChanges();
                return "Product Deleted";
            }
            else
                return "Error";
        }

        // Cart Services

        public void AddProductToCart(int productId)
        {
            var product = _dbContext.Products.SingleOrDefault(m => m.Id == productId);
            CartItem item = new CartItem();
            item.Name = product.ProductName;
            item.ImageUrl = product.ImageUrl;
            item.ProductId = product.Id;
            item.Price = product.Price;
            item.Copies = 1;
            if (_httpContextAccessor.HttpContext.Session.GetString("ProductCart") == null)
            {
                CartItems.Add(item);
                _httpContextAccessor.HttpContext.Session.SetString("ProductCart", JsonConvert.SerializeObject(CartItems));
            }
            else
            {
                var session = _httpContextAccessor.HttpContext.Session;
                List<CartItem> cartItems = JsonConvert.DeserializeObject<List<CartItem>>(session.GetString("ProductCart"));
                //var ismovie = (CartItem)cartItems[0];
                var isProduct = cartItems.Where(c => c.ProductId == productId).FirstOrDefault();
                if (isProduct != null)
                {
                    isProduct.Copies = isProduct.Copies + 1;
                }
                else
                {
                    cartItems.Add(item);
                }
                _httpContextAccessor.HttpContext.Session.SetString("ProductCart", JsonConvert.SerializeObject(cartItems));
            }
        }

        public List<CartItem>? DisplayCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            if (_httpContextAccessor.HttpContext.Session.GetString("ProductCart") != null)
            {
                List<CartItem>? cartitems = JsonConvert.DeserializeObject<List<CartItem>>(session.GetString("ProductCart"));
                return cartitems;
            }
            else
            {
                List<CartItem>? cartitems = new List<CartItem>();
                return cartitems;
            }
        }

        public Task<string> UserCheckout(string email)
        {
            var user = _userManager.FindByEmailAsync(email).Result;
            var session = _httpContextAccessor.HttpContext.Session;

            Order orderobj = new Order();
            orderobj.OrderDate = DateTime.Today.ToShortDateString();
            orderobj.User = user;
            if (user != null)
            {
                _dbContext.Orders.Add(orderobj);
                _dbContext.SaveChanges();

                List<CartItem>? cartitems = JsonConvert.DeserializeObject<List<CartItem>>(session.GetString("ProductCart"));
                foreach (var item in cartitems)
                {
                    OrderRow Orderrow = new OrderRow();
                    var product = _dbContext.Products
                                .Where(p => p.Id == item.ProductId)
                                .FirstOrDefault();
                    Orderrow.Product = product;
                    Orderrow.Price = product.Price;
                    Orderrow.Order = orderobj;
                    _dbContext.OrderRows.Add(Orderrow);
                    _dbContext.SaveChanges();
                }
                string response = "";
                var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                var config = builder.Build();
                var message = new MimeMessage();
                message.To.Add(MailboxAddress.Parse(email));
                message.From.Add(new MailboxAddress("Digital Dragons", "digitaldragons571@gmail.com"));
                message.Subject = "Order Confirmation Email";
                //We will say we are sending HTML. But there are options for plaintext etc.
                string content = "";
                content = "<b>" + "Hello\n" + user.FirstName + " " + user.LastName + "," + "<b>" + "<br/><br/>" +
                        "Thank you for Shopping with us.";
                message.Body = new TextPart(TextFormat.Html) { Text = content };
                //Be careful that the SmtpClient class is the one from Mailkit not the framework!
                using (var emailClient = new SmtpClient())
                {
                    try
                    {
                        emailClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                        //The last parameter here is to use SSL (Which you should!)
                        emailClient.Connect(config["SmtpServer"], Convert.ToInt32(config["SmtpPort"]), true);
                    }
                    catch (SmtpCommandException ex)
                    {
                        response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
                        return Task.FromResult(response);
                    }
                    catch (SmtpProtocolException ex)
                    {
                        response = "Protocol error while trying to connect:" + ex.Message;
                        return Task.FromResult(response);
                    }
                    //Remove any OAuth functionality as we won't be using it.
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    emailClient.Authenticate(config["SmtpUsername"], config["SmtpPassword"]);
                    try
                    {
                        emailClient.Send(message);
                    }
                    catch (SmtpCommandException ex)
                    {
                        response = "Error sending message: " + ex.Message + " StatusCode: " + ex.StatusCode;
                        switch (ex.ErrorCode)
                        {
                            case SmtpErrorCode.RecipientNotAccepted:
                                response += " Recipient not accepted: " + ex.Mailbox;
                                break;

                            case SmtpErrorCode.SenderNotAccepted:
                                response += " Sender not accepted: " + ex.Mailbox;
                                Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                                break;

                            case SmtpErrorCode.MessageNotAccepted:
                                response += " Message not accepted.";
                                break;
                        }
                    }
                    emailClient.Disconnect(true);
                }
                session.Remove("ProductCart");
                session.Remove("ShoppingCart");
                return Task.FromResult("success");
            }
            else { return Task.FromResult("danger"); }
        }
    
        public void IncreaseCopy(int id)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            List<CartItem>? cartitems = JsonConvert.DeserializeObject<List<CartItem>>(session.GetString("ProductCart"));
            var product = _dbContext.Products.SingleOrDefault(p => p.Id == id);
            CartItem item = new CartItem();
            item.Name = product.ProductName;
            item.ImageUrl = product.ImageUrl;
            item.ProductId = product.Id;
            item.Price = product.Price;
            item.Copies = 1;
            cartitems.Add(item);
            _httpContextAccessor.HttpContext.Session.SetString("ProductCart", JsonConvert.SerializeObject(cartitems));

            //Display the count of cart
            var cartList = _httpContextAccessor.HttpContext.Session.Get<List<int>>("ShoppingCart") ?? new List<int> { };
            cartList.Add(id); //add the movieId to the list
            _httpContextAccessor.HttpContext.Session.Set<List<int>>("ShoppingCart", cartList);
        }

        public void DecreaseCopy(int id)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            List<CartItem>? cartitems = JsonConvert.DeserializeObject<List<CartItem>>(session.GetString("ProductCart"));
            var item = cartitems.FirstOrDefault(p => p.ProductId == id);
            cartitems.Remove(item);
            _httpContextAccessor.HttpContext.Session.SetString("ProductCart", JsonConvert.SerializeObject(cartitems));

            var cartList = _httpContextAccessor.HttpContext.Session.Get<List<int>>("ShoppingCart") ?? new List<int> { };
            cartList.Remove(id); //add the movieId to the list
            _httpContextAccessor.HttpContext.Session.Set<List<int>>("ShoppingCart", cartList);
        }

        public List<UserOrderVM> GetUsersOrders(string email)
        {
            var userOrderList = _dbContext.Orders
                                    .Include(o => o.User)
                                    .Include(o => o.OrderRowList)
                                    .ThenInclude(or => or.Product)
                                    .Where(o => o.User.Email == email)
                                    .Select(o => new UserOrderVM
                                    {
                                        Firstname = o.User.FirstName,
                                        Lastname = o.User.LastName,
                                        DateOfPurchase = o.OrderDate,
                                        OrderID = o.Id,
                                        Products = o.OrderRowList.Select(or => new ProductVM
                                        {
                                            Name = or.Product.ProductName,
                                            Price = or.Product.Price
                                        }).ToList(),
                                        TotalOrderCost = o.OrderRowList.Sum(or => or.Product.Price),
                                        TotalOrderCount = o.OrderRowList.Count()

                                    }).ToList();

            return userOrderList;
        }
    }
}
