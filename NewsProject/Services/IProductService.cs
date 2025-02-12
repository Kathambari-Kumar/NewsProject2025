using NewsProject.Models.DB;
using NewsProject.Models.VM;
using SixLabors.ImageSharp.Formats;

namespace NewsProject.Services
{
    public interface IProductService
    {
        public string CreateProduct(Product product);
        public List<Product> GetProductDetails();
        public Product GetProductById(int productId);
        public string UpdateProduct(Product product);
        public string DeleteProduct(int productId);
        public void AddProductToCart(int movieid);
        public List<CartItem>? DisplayCart();
        public void DecreaseCopy(int id);
        public Task<string> UserCheckout(string email);
        public void IncreaseCopy(int id);
        public List<UserOrderVM> GetUsersOrders(string email);
    }
}
